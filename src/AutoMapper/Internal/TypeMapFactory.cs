using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class TypeMapFactory : ITypeMapFactory
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();
        private static readonly Internal.IDictionary<Type, TypeInfo> _typeInfos =
            DictionaryFactory.CreateDictionary<Type, TypeInfo>();

        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions options, MemberList memberList)
        {
            var sourceTypeInfo = GetTypeInfo(sourceType);
            var destTypeInfo = GetTypeInfo(destinationType);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList);

            foreach (var destProperty in destTypeInfo.GetPublicWriteAccessors())
            {
                var members = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(members, sourceTypeInfo, destProperty.Name, options))
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

                if (!MapDestinationPropertyToSource(members, sourceTypeInfo, parameter.Name, options))
                    return false;

                var resolvers = members.Select(mi => mi.ToMemberGetter());

                var param = new ConstructorParameterMap(parameter, resolvers.ToArray());

                parameters.Add(param);
            }

            typeMap.AddConstructorMap(destCtor, parameters);

            return true;
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            TypeInfo typeInfo = _typeInfos.GetOrAdd(type, t => new TypeInfo(type));

            return typeInfo;
        }

        private bool MapDestinationPropertyToSource(LinkedList<MemberInfo> resolvers, TypeInfo sourceType,
                                                    string nameToSearch, IMappingOptions mappingOptions)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;

            var sourceProperties = sourceType.GetPublicReadAccessors();
            var sourceNoArgMethods = sourceType.GetPublicNoArgMethods();
			var sourceNoArgExtensionMethods = sourceType.GetPublicNoArgExtensionMethods(mappingOptions.SourceExtensionMethodSearch);

			MemberInfo resolver = FindTypeMember(sourceProperties, sourceNoArgMethods, sourceNoArgExtensionMethods, nameToSearch, mappingOptions);

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
                                                                    GetTypeInfo(valueResolver.GetMemberType()),
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
            var possibleSourceNames = PossibleNames(memberName, mappingOptions.Aliases, mappingOptions.Prefixes,
                                                          mappingOptions.Postfixes);
            var possibleDestNames = PossibleNames(nameToMatch, mappingOptions.Aliases, mappingOptions.DestinationPrefixes,
                                                          mappingOptions.DestinationPostfixes);

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new {sourceName, destName};

            return all.Any(pair => String.Compare(pair.sourceName, pair.destName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static IEnumerable<string> PossibleNames(string memberName, IEnumerable<AliasedMember> aliases, IEnumerable<string> prefixes, IEnumerable<string> postfixes)
        {
            if (string.IsNullOrEmpty(memberName))
                yield break;

            yield return memberName;

            foreach (var alias in aliases.Where(alias => String.Equals(memberName, alias.Member, StringComparison.Ordinal)))
            {
                yield return alias.Alias;
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