namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class TypeMapFactory : ITypeMapFactory
    {
        /// <summary>
        /// 
        /// </summary>
        private static IDictionaryFactory DictionaryFactory { get; }
            = PlatformAdapter.Resolve<IDictionaryFactory>();

        /// <summary>
        /// 
        /// </summary>
        private Internal.IDictionary<Type, TypeInfo> TypeInfos { get; }
            = DictionaryFactory.CreateDictionary<Type, TypeInfo>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="options"></param>
        /// <param name="memberList"></param>
        /// <returns></returns>
        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions options,
            MemberList memberList)
        {
            var sourceTypeInfo = GetTypeInfo(sourceType, options.SourceExtensionMethods);
            var destTypeInfo = GetTypeInfo(destinationType, new MethodInfo[0]);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var members = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(members, sourceTypeInfo, destProperty.Name, options))
                {
                    var resolvers = members.Select(mi => mi.ToMemberGetter());
                    var destPropertyAccessor = destProperty.ToMemberAccessor();

                    ////TODO: turns out this is redundant
                    //typeMap.AddPropertyMap(destPropertyAccessor, resolvers.Cast<IValueResolver>());
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        /// <param name="destCtor"></param>
        /// <param name="sourceTypeInfo"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="extensionMethodsToSearch"></param>
        /// <returns></returns>
        private TypeInfo GetTypeInfo(Type type, IEnumerable<MethodInfo> extensionMethodsToSearch)
        {
            var typeInfo = TypeInfos.GetOrAdd(type, t => new TypeInfo(type, extensionMethodsToSearch));
            return typeInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolvers"></param>
        /// <param name="sourceType"></param>
        /// <param name="nameToSearch"></param>
        /// <param name="mappingOptions"></param>
        /// <returns></returns>
        private bool MapDestinationPropertyToSource(LinkedList<MemberInfo> resolvers, TypeInfo sourceType,
            string nameToSearch, IMappingOptions mappingOptions)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;

            var sourceProperties = sourceType.PublicReadAccessors;
            var sourceNoArgMethods = sourceType.PublicNoArgMethods;
            var sourceNoArgExtensionMethods = sourceType.PublicNoArgExtensionMethods;

            MemberInfo resolver = FindTypeMember(sourceProperties, sourceNoArgMethods, sourceNoArgExtensionMethods,
                nameToSearch, mappingOptions);

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

                    var valueResolver = FindTypeMember(sourceProperties, sourceNoArgMethods, sourceNoArgExtensionMethods,
                        snippet.First,
                        mappingOptions);

                    if (valueResolver != null)
                    {
                        resolvers.AddLast(valueResolver);

                        foundMatch = MapDestinationPropertyToSource(resolvers,
                            GetTypeInfo(valueResolver.GetMemberType(), mappingOptions.SourceExtensionMethods),
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelProperties"></param>
        /// <param name="getMethods"></param>
        /// <param name="getExtensionMethods"></param>
        /// <param name="nameToSearch"></param>
        /// <param name="mappingOptions"></param>
        /// <returns></returns>
        private static MemberInfo FindTypeMember(IEnumerable<MemberInfo> modelProperties,
            IEnumerable<MethodInfo> getMethods,
            IEnumerable<MethodInfo> getExtensionMethods,
            string nameToSearch,
            IMappingOptions mappingOptions)
        {
            var pi = modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, nameToSearch, mappingOptions));
            if (pi != null) return pi;

            var mi = getMethods.FirstOrDefault(m => NameMatches(m.Name, nameToSearch, mappingOptions));
            if (mi != null) return mi;

            mi = getExtensionMethods.FirstOrDefault(m => NameMatches(m.Name, nameToSearch, mappingOptions));
            return mi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="nameToMatch"></param>
        /// <param name="mappingOptions"></param>
        /// <returns></returns>
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
                all.Any(pair => string.Compare(pair.sourceName, pair.destName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="aliases"></param>
        /// <param name="memberNameReplacers"></param>
        /// <param name="prefixes"></param>
        /// <param name="postfixes"></param>
        /// <returns></returns>
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

            if (memberNameReplacers.Any())
            {
                var aliasName = memberName;

                foreach (var nameReplacer in memberNameReplacers)
                {
                    aliasName = aliasName.Replace(nameReplacer.OriginalValue, nameReplacer.NewValue);
                }

                yield return aliasName;
            }

            foreach (var prefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.Ordinal)))
            {
                var withoutPrefix = memberName.Substring(prefix.Length);

                yield return withoutPrefix;

                foreach (
                    var postfix in postfixes.Where(postfix => withoutPrefix.EndsWith(postfix, StringComparison.Ordinal))
                    )
                {
                    yield return withoutPrefix.Remove(withoutPrefix.Length - postfix.Length);
                }
            }

            foreach (var postfix in postfixes.Where(postfix => memberName.EndsWith(postfix, StringComparison.Ordinal)))
            {
                yield return memberName.Remove(memberName.Length - postfix.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matches"></param>
        /// <param name="i"></param>
        /// <param name="mappingOptions"></param>
        /// <returns></returns>
        private static NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i, IMappingOptions mappingOptions)
        {
            return new NameSnippet
            {
                First =
                    string.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                        matches.Take(i).ToArray()),
                Second =
                    string.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter,
                        matches.Skip(i).ToArray())
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private class NameSnippet
        {
            /// <summary>
            /// 
            /// </summary>
            public string First { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Second { get; set; }
        }
    }
}