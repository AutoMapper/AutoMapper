using System.Collections.Concurrent;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Internal;

    public class TypeMapFactory : ITypeMapFactory
    {
        private static readonly ConcurrentDictionary<Type, TypeDetails> _typeInfos
            = new ConcurrentDictionary<Type, TypeDetails>();

        public static TypeDetails GetTypeInfo(Type type, IProfileConfiguration profileConfiguration)
        {
            TypeDetails typeInfo = _typeInfos.GetOrAdd(type, t => new TypeDetails(type, profileConfiguration.ShouldMapProperty, profileConfiguration.ShouldMapField, profileConfiguration.SourceExtensionMethods));

            return typeInfo;
        }

        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IProfileConfiguration options, MemberList memberList)
        {
            var sourceTypeInfo = GetTypeInfo(sourceType, options);
            var destTypeInfo = GetTypeInfo(destinationType, options);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList, options.ProfileName);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var resolvers = new LinkedList<IValueResolver>();

                if (MapDestinationPropertyToSource(options, sourceTypeInfo, destProperty.GetMemberType(), destProperty.Name, resolvers))
                {
                    var destPropertyAccessor = destProperty.ToMemberAccessor();

                    typeMap.AddPropertyMap(destPropertyAccessor, resolvers);
                }
            }
            if (!destinationType.IsAbstract() && destinationType.IsClass())
            {
                foreach (var destCtor in destTypeInfo.Constructors.OrderByDescending(ci => ci.GetParameters().Length))
                {
                    if (MapDestinationCtorToSource(typeMap, destCtor, sourceTypeInfo, options))
                    {
                        break;
                    }
                }
            }
            return typeMap;
        }

        private bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceTypeInfo, Type destType, string destMemberInfo, LinkedList<IValueResolver> members)
        {
            return options.MemberConfigurations.Any(_ => _.MapDestinationPropertyToSource(options, sourceTypeInfo, destType, destMemberInfo, members));
        }

        private bool MapDestinationCtorToSource(TypeMap typeMap, ConstructorInfo destCtor, TypeDetails sourceTypeInfo, IProfileConfiguration options)
        {
            var parameters = new List<ConstructorParameterMap>();
            var ctorParameters = destCtor.GetParameters();

            if (ctorParameters.Length == 0 || !options.ConstructorMappingEnabled)
                return false;

            foreach (var parameter in ctorParameters)
            {
                var resolvers = new LinkedList<IValueResolver>();

                var canResolve = MapDestinationPropertyToSource(options, sourceTypeInfo, parameter.GetType(), parameter.Name, resolvers);
                if(!canResolve && parameter.HasDefaultValue)
                {
                    canResolve = true;
                }

                var param = new ConstructorParameterMap(parameter, resolvers.ToArray(), canResolve);

                parameters.Add(param);
            }

            typeMap.AddConstructorMap(destCtor, parameters);

            return true;
        }

        public TypeDetails GetTypeInfo(Type type, IMappingOptions mappingOptions)
        {
            return GetTypeInfo(type, mappingOptions.ShouldMapProperty, mappingOptions.ShouldMapField, mappingOptions.SourceExtensionMethods);
        }

        private TypeDetails GetTypeInfo(Type type, Func<PropertyInfo, bool> shouldMapProperty, Func<FieldInfo, bool> shouldMapField, IEnumerable<MethodInfo> extensionMethodsToSearch)
        {
            return _typeInfos.GetOrAdd(type, t => new TypeDetails(type, shouldMapProperty, shouldMapField, extensionMethodsToSearch));
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
            var possibleSourceNames = PossibleNames(memberName, mappingOptions.Aliases,
                mappingOptions.MemberNameReplacers,
                mappingOptions.Prefixes, mappingOptions.Postfixes);

            var possibleDestNames = PossibleNames(nameToMatch, mappingOptions.Aliases,
                mappingOptions.MemberNameReplacers,
                mappingOptions.DestinationPrefixes, mappingOptions.DestinationPostfixes);

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new {sourceName, destName};

            return
                all.Any(pair => String.Compare(pair.sourceName, pair.destName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static IEnumerable<string> PossibleNames(string memberName, IEnumerable<AliasedMember> aliases,
            IEnumerable<MemberNameReplacer> memberNameReplacers, IEnumerable<string> prefixes,
            IEnumerable<string> postfixes)
        {
            if (string.IsNullOrEmpty(memberName))
                yield break;

            yield return memberName;

            foreach (
                var alias in aliases.Where(alias => String.Equals(memberName, alias.Member, StringComparison.Ordinal)))
            {
                yield return alias.Alias;
            }
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