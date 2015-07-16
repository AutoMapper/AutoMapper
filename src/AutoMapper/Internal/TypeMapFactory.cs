using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class TypeMapFactory : ITypeMapFactory
    {
        private static readonly Internal.IDictionary<Type, TypeInfo> _typeInfos
            = PlatformAdapter.Resolve<IDictionaryFactory>().CreateDictionary<Type, TypeInfo>();

        internal static TypeInfo GetTypeInfo(Type type, IEnumerable<MethodInfo> extensionMethodsToSearch)
        {
            TypeInfo typeInfo = _typeInfos.GetOrAdd(type, t => new TypeInfo(type, extensionMethodsToSearch));

            return typeInfo;
        }
        internal static ICollection<ISourceToDestinationMemberMapper> sourceToDestinationMemberMappers = new Collection<ISourceToDestinationMemberMapper>
        {
            // Need to do it fixie way for prefix and postfix to work together + not specify match explicitly
            // Have 3 properties for Members, Methods, And External Methods
            // Parent goes to all
            new ParentSourceToDestinationMemberMapper().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.SetPrefixs("Get")).SetMemberInfo<AllMemberInfo>(),
            //new CustomizedSourceToDestinationMemberMapper().MemberNameMatch().ExtensionNameMatch().ExtensionPrefix("Get").MethodPrefix("Get").MethodNameMatch(),
        };

        

        internal static readonly ICollection<ISourceToDestinationMemberMapper> def = sourceToDestinationMemberMappers.ToList();

        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions options, MemberList memberList)
        {
            var sourceTypeInfo = GetTypeInfo(sourceType, options.SourceExtensionMethods);
            var destTypeInfo = GetTypeInfo(destinationType, options.SourceExtensionMethods);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList);
                
            foreach (var destProperty in destTypeInfo.GetPublicWriteAccessors())
            {
                var members = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(sourceTypeInfo, destProperty.Name, members))
                {
                    var resolvers = members.Select(mi => mi.ToMemberGetter());
                    var destPropertyAccessor = destProperty.ToMemberAccessor();

                    typeMap.AddPropertyMap(destPropertyAccessor, resolvers.Cast<IValueResolver>());
                }
            }
            if (!destinationType.IsAbstract && destinationType.IsClass)
            {
                foreach (var destCtor in destTypeInfo.GetConstructors().OrderByDescending(ci => ci.GetParameters().Length))
                {
                    if (MapDestinationCtorToSource(typeMap, destCtor, sourceTypeInfo, options))
                    {
                        break;
                    }
                }
            }
            return typeMap;
        }

        private bool MapDestinationPropertyToSource(TypeInfo sourceTypeInfo, string name, LinkedList<MemberInfo> members)
        {
            return sourceToDestinationMemberMappers.Any(s => s.MapDestinationPropertyToSource(sourceTypeInfo, name, members));
        }

        private bool MapDestinationCtorToSource(TypeMap typeMap, ConstructorInfo destCtor, TypeInfo sourceTypeInfo,
                                                IMappingOptions options)
        {
            var parameters = new List<ConstructorParameterMap>();
            var ctorParameters = destCtor.GetParameters();

            if (ctorParameters.Length == 0 || !options.ConstructorMappingEnabled)
                return false;

            foreach (var parameter in ctorParameters)
            {
                var members = new LinkedList<MemberInfo>();

                if (!MapDestinationPropertyToSource(sourceTypeInfo, parameter.Name, members))
                    return false;

                var resolvers = members.Select(mi => mi.ToMemberGetter());

                var param = new ConstructorParameterMap(parameter, resolvers.ToArray());

                parameters.Add(param);
            }

            typeMap.AddConstructorMap(destCtor, parameters);

            return true;
        }

    }

    

    public abstract class MappingOptionsMemberMapperBase
    {
        public abstract IEnumerable<Func<TypeInfo, string, MemberInfo>> Convensions { get; }

        public bool MapDestinationPropertyToSource(LinkedList<MemberInfo> resolvers, TypeInfo sourceType, string nameToSearch)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = MatchingMemberInfo(sourceType, nameToSearch);

            if (matchingMemberInfo != null)
            {
                resolvers.AddLast(matchingMemberInfo);
                return true;
            }

            string[] matches = DestinationMemberNamingConvention.SplittingExpression
                    .Matches(nameToSearch)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToArray();
            for (int i = 1; (i <= matches.Length) && (matchingMemberInfo == null); i++)
            {
                NameSnippet snippet = CreateNameSnippet(matches, i);

                matchingMemberInfo = MatchingMemberInfo(sourceType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo);

                    var foundMatch = MapDestinationPropertyToSource(resolvers, TypeMapFactory.GetTypeInfo(matchingMemberInfo.GetMemberType(), SourceExtensionMethods), snippet.Second);

                    if (!foundMatch)
                    {
                        resolvers.RemoveLast();
                    }
                }
            }

            return matchingMemberInfo != null;
        }
        private MemberInfo MatchingMemberInfo(TypeInfo sourceType, string nameToSearch)
        {
            MemberInfo matchingMemberInfo = null;
            var namesToSearch =
                MemberNameReplacers.Select(r => nameToSearch.Replace(r.OrginalValue, r.NewValue))
                    .Concat(new[] { MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OrginalValue, r.NewValue)) })
                    .ToList();

            foreach (var name in namesToSearch)
            {
                foreach (var convension in Convensions)
                {
                    matchingMemberInfo = convension(sourceType, name);
                    if (matchingMemberInfo != null)
                        break;
                }
                if (matchingMemberInfo != null)
                    break;
            }
            return matchingMemberInfo;
        }

        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i)
        {
            return new NameSnippet
            {
                First = String.Join(SourceMemberNamingConvention.SeparatorCharacter,matches.Take(i).ToArray()),
                Second = String.Join(SourceMemberNamingConvention.SeparatorCharacter,matches.Skip(i).ToArray())
            };
        }

        protected class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }

        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }
        public IEnumerable<string> Prefixes { get; private set; }
        public IEnumerable<string> Postfixes { get; private set; }
        public IEnumerable<string> DestinationPrefixes { get; private set; }
        public IEnumerable<string> DestinationPostfixes { get; private set; }
        public IEnumerable<MemberNameReplacer> MemberNameReplacers { get; private set; }
        public IEnumerable<AliasedMember> Aliases { get; private set; }
        public bool ConstructorMappingEnabled { get; private set; }
        public bool DataReaderMapperYieldReturnEnabled { get; private set; }
        public IEnumerable<MethodInfo> SourceExtensionMethods { get; private set; }
    }


    public class CustomizedSourceToDestinationMemberMapper
    {
        private readonly ICollection<Func<TypeInfo, string, IMappingOptions, MemberInfo>> _convensions = new Collection<Func<TypeInfo, string, IMappingOptions, MemberInfo>>();

        public ICollection<Func<TypeInfo, string, IMappingOptions, MemberInfo>> Convensions {
            get { return _convensions; }
        }

        public ICollection<MemberNameReplacer> MemberNameReplacers { get; private set; }

        public CustomizedSourceToDestinationMemberMapper()
        {
            MemberNameReplacers = new Collection<MemberNameReplacer>();
        }

        public bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers, ISourceToDestinationMemberMapper parent)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            return false;
            //var matchingMemberInfo = MatchingMemberInfo(sourceType, nameToSearch);

            //if (matchingMemberInfo != null)
            //{
            //    resolvers.AddLast(matchingMemberInfo);
            //    return true;
            //}

            //string[] matches = DestinationMemberNamingConvention.SplittingExpression
            //        .Matches(nameToSearch)
            //        .Cast<Match>()
            //        .Select(m => m.Value)
            //        .ToArray();
            //for (int i = 1; (i <= matches.Length) && (matchingMemberInfo == null); i++)
            //{
            //    NameSnippet snippet = CreateNameSnippet(matches, i);

            //    matchingMemberInfo = MatchingMemberInfo(sourceType, snippet.First);

            //    if (matchingMemberInfo != null)
            //    {
            //        resolvers.AddLast(matchingMemberInfo);

            //        var foundMatch = MapDestinationPropertyToSource(resolvers, TypeMapFactory.GetTypeInfo(matchingMemberInfo.GetMemberType(), mappingOptions.SourceExtensionMethods), snippet.Second, mappingOptions);

            //        if (!foundMatch)
            //        {
            //            resolvers.RemoveLast();
            //        }
            //    }
            //}

            //return matchingMemberInfo != null;
        }

        private MemberInfo MatchingMemberInfo(TypeInfo sourceType, string nameToSearch, IMappingOptions mappingOptions)
        {
            MemberInfo matchingMemberInfo = null;
            var namesToSearch =
                MemberNameReplacers.Select(r => nameToSearch.Replace(r.OrginalValue, r.NewValue))
                    .Concat(new[] {MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OrginalValue, r.NewValue)), nameToSearch})
                    .ToList();

            foreach (var name in namesToSearch)
            {
                foreach (var convension in _convensions)
                {
                    matchingMemberInfo = convension(sourceType, name, mappingOptions);
                    if (matchingMemberInfo != null)
                        break;
                }
                if (matchingMemberInfo != null)
                    break;
            }
            return matchingMemberInfo;
        }

        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i, IMappingOptions mappingOptions)
        {
            return new NameSnippet
            {
                First =
                    String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Take(i).ToArray()),
                Second =
                    String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Skip(i).ToArray())
            };
        }

        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }

    }

    public static class CustomizedMemberMapperExtensions
    {
        public static CustomizedSourceToDestinationMemberMapper Prefix(this CustomizedSourceToDestinationMemberMapper self, params string[] prefix)
        {
            return self.MemberPrefix(prefix).MethodPrefix(prefix).ExtensionPrefix(prefix);
        }
        public static CustomizedSourceToDestinationMemberMapper MemberPrefix(this CustomizedSourceToDestinationMemberMapper self, params string[] prefix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicReadAccessors().FirstOrDefault(mi => HasPrefix(mi, name, prefix)));
            return self;
        }
        public static CustomizedSourceToDestinationMemberMapper MethodPrefix(this CustomizedSourceToDestinationMemberMapper self, params string[] prefix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgMethods().FirstOrDefault(mi => HasPrefix(mi, name, prefix)));
            return self;
        }
        public static CustomizedSourceToDestinationMemberMapper ExtensionPrefix(this CustomizedSourceToDestinationMemberMapper self, params string[] prefix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgExtensionMethods().FirstOrDefault(mi => HasPrefix(mi, name, prefix)));
            return self;
        }
        private static bool HasPrefix( MemberInfo mi, string name, params string[] prefix)
        {
            return prefix.Any(p => string.CompareOrdinal(mi.Name, p + name) == 0 || string.CompareOrdinal(name, p + mi.Name) == 0);
        }

        public static CustomizedSourceToDestinationMemberMapper MemberNameMatch(this CustomizedSourceToDestinationMemberMapper self)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicReadAccessors().FirstOrDefault(mi => NamesMatch(mi, name)));
            return self;
        }

        public static CustomizedSourceToDestinationMemberMapper MethodNameMatch(this CustomizedSourceToDestinationMemberMapper self)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgMethods().FirstOrDefault(mi => NamesMatch(mi, name)));
            return self;
        }

        public static CustomizedSourceToDestinationMemberMapper ExtensionNameMatch(this CustomizedSourceToDestinationMemberMapper self)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgExtensionMethods().FirstOrDefault(mi => NamesMatch(mi, name)));
            return self;
        }

        private static bool NamesMatch(MemberInfo mi, string name)
        {
            return string.CompareOrdinal(mi.Name, name) == 0;
        }

        public static CustomizedSourceToDestinationMemberMapper MemberPostfix(this CustomizedSourceToDestinationMemberMapper self, params string[] postfix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicReadAccessors().FirstOrDefault(mi => HasPostfix(mi, name, postfix)));
            return self;
        }
        public static CustomizedSourceToDestinationMemberMapper MethodPostfix(this CustomizedSourceToDestinationMemberMapper self, params string[] postfix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgMethods().FirstOrDefault(mi => HasPostfix(mi, name, postfix)));
            return self;
        }
        public static CustomizedSourceToDestinationMemberMapper ExtensionPostfix(this CustomizedSourceToDestinationMemberMapper self, params string[] postfix)
        {
            self.Convensions.Add((ti, name, mo) => ti.GetPublicNoArgExtensionMethods().FirstOrDefault(mi => HasPostfix(mi, name, postfix)));
            return self;
        }

        private static bool HasPostfix(MemberInfo mi, string name, params string[] postFix)
        {
            return postFix.Any(p => mi.Name == name + p || name == mi.Name + p);
        }

        public static CustomizedSourceToDestinationMemberMapper Where(this CustomizedSourceToDestinationMemberMapper self, Func<TypeInfo, string, IMappingOptions, MemberInfo> whereClaulse)
        {
            self.Convensions.Add(whereClaulse);
            return self;
        }
    }

    public class DefaultPropertyToSourceMappings
    {

        public bool MapDestinationPropertyToSource(LinkedList<MemberInfo> resolvers, TypeInfo sourceType,
                                                    string nameToSearch, IMappingOptions mappingOptions)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;

            var sourceProperties = sourceType.GetPublicReadAccessors();
            var sourceNoArgMethods = sourceType.GetPublicNoArgMethods();
            var sourceNoArgExtensionMethods = sourceType.GetPublicNoArgExtensionMethods();

            MemberInfo resolver = null;// FindTypeMember(sourceProperties, sourceNoArgMethods, sourceNoArgExtensionMethods, nameToSearch, mappingOptions);

            bool foundMatch = resolver != null;

            if (foundMatch)
            {
                resolvers.AddLast(resolver);
            }
            else
            {
                string[] matches = mappingOptions.DestinationMemberNamingConvention.SplittingExpression
                    .Matches(nameToSearch)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToArray();

                for (int i = 1; (i <= matches.Length) && (!foundMatch); i++)
                {
                    NameSnippet snippet = CreateNameSnippet(matches, i, mappingOptions);

                    var valueResolver = FindTypeMember(sourceProperties, sourceNoArgMethods, sourceNoArgExtensionMethods, snippet.First,
                                                       mappingOptions);

                    if (valueResolver != null)
                    {
                        resolvers.AddLast(valueResolver);

                        foundMatch = MapDestinationPropertyToSource(resolvers,
                                                                    TypeMapFactory.GetTypeInfo(valueResolver.GetMemberType(), mappingOptions.SourceExtensionMethods),
                                                                    snippet.Second, mappingOptions);

                        if (!foundMatch)
                        {
                            resolvers.RemoveLast();
                        }
                    }
                }
            }

            return foundMatch;
        }

        private static MemberInfo FindTypeMember(IEnumerable<MemberInfo> modelProperties,
                                                 IEnumerable<MethodInfo> getMethods,
                                                 IEnumerable<MethodInfo> getExtensionMethods,
                                                 string nameToSearch,
                                                 IMappingOptions mappingOptions)
        {
            MemberInfo pi = modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, nameToSearch, mappingOptions));
            if (pi != null)
                return pi;

            MethodInfo mi = getMethods.FirstOrDefault(m => NameMatches(m.Name, nameToSearch, mappingOptions));
            if (mi != null)
                return mi;

            mi = getExtensionMethods.FirstOrDefault(m => NameMatches(m.Name, nameToSearch, mappingOptions));
            if (mi != null)
                return mi;

            return null;
        }

        private static bool NameMatches(string memberName, string nameToMatch, IMappingOptions mappingOptions)
        {
            var possibleSourceNames = PossibleNames(memberName, mappingOptions.Aliases, mappingOptions.MemberNameReplacers,
                mappingOptions.Prefixes, mappingOptions.Postfixes);

            var possibleDestNames = PossibleNames(nameToMatch, mappingOptions.Aliases, mappingOptions.MemberNameReplacers,
                mappingOptions.DestinationPrefixes, mappingOptions.DestinationPostfixes);

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };

            return all.Any(pair => String.Compare(pair.sourceName, pair.destName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static IEnumerable<string> PossibleNames(string memberName, IEnumerable<AliasedMember> aliases,
            IEnumerable<MemberNameReplacer> memberNameReplacers, IEnumerable<string> prefixes, IEnumerable<string> postfixes)
        {
            if (string.IsNullOrEmpty(memberName))
                yield break;

            yield return memberName;

            foreach (var alias in aliases.Where(alias => String.Equals(memberName, alias.Member, StringComparison.Ordinal)))
            {
                yield return alias.Alias;
            }
            yield break;

            if (memberNameReplacers.Any())
            {
                string aliasName = memberName;

                foreach (var nameReplacer in memberNameReplacers)
                {
                    aliasName = aliasName.Replace(nameReplacer.OrginalValue, nameReplacer.NewValue);
                }

                yield return aliasName;
            }

            foreach (var prefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.Ordinal)))
            {
                var withoutPrefix = memberName.Substring(prefix.Length);

                yield return withoutPrefix;

                foreach (var postfix in postfixes.Where(postfix => withoutPrefix.EndsWith(postfix, StringComparison.Ordinal)))
                {
                    yield return withoutPrefix.Remove(withoutPrefix.Length - postfix.Length);
                }
            }

            foreach (var postfix in postfixes.Where(postfix => memberName.EndsWith(postfix, StringComparison.Ordinal)))
            {
                yield return memberName.Remove(memberName.Length - postfix.Length);
            }
        }

        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i, IMappingOptions mappingOptions)
        {
            return new NameSnippet
            {

                First =
                    String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Take(i).ToArray()),
                Second =
                    String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Skip(i).ToArray())
            };
        }

        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }

    }

}