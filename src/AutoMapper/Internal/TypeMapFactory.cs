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

                if (MapDestinationPropertyToSource(sourceTypeInfo, destProperty.GetType(), destProperty.Name, members))
                {
                    var resolvers = members.Select(mi => mi.ToMemberGetter());
                    var destPropertyAccessor = destProperty.ToMemberAccessor();

                    typeMap.AddPropertyMap(destPropertyAccessor, resolvers.Cast<IValueResolver>());
                }
            }
            if (!destinationType.IsAbstract() && destinationType.IsClass())
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

        private bool MapDestinationPropertyToSource(TypeInfo sourceTypeInfo, Type destType, string destMemberInfo, LinkedList<MemberInfo> members)
        {
            return sourceToDestinationMemberMappers.Any(s => s.MapDestinationPropertyToSource(sourceTypeInfo, destType, destMemberInfo, members));
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

                if (!MapDestinationPropertyToSource(sourceTypeInfo, parameter.GetType(), parameter.Name, members))
                    return false;

                var resolvers = members.Select(mi => mi.ToMemberGetter());

                var param = new ConstructorParameterMap(parameter, resolvers.ToArray());

                parameters.Add(param);
            }

            typeMap.AddConstructorMap(destCtor, parameters);

            return true;
        }

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
                    String.Join("",matches.Take(i).ToArray()),
                Second =
                    String.Join("",matches.Skip(i).ToArray())
            };
        }

        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }

    }
}