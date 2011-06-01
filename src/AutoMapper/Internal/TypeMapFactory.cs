using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class TypeMapFactory : ITypeMapFactory
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> _typeInfos = new ConcurrentDictionary<Type, TypeInfo>();

        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions options)
        {
            var sourceTypeInfo = GetTypeInfo(sourceType);
            var destTypeInfo = GetTypeInfo(destinationType);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo);

            foreach (var destProperty in destTypeInfo.GetPublicWriteAccessors())
            {
                var members = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(members, sourceTypeInfo, destProperty.Name, options))
                {
                    var resolvers = members.Select(mi => mi.ToMemberGetter());
                    var destPropertyAccessor = destProperty.ToMemberAccessor();
                    typeMap.AddPropertyMap(destPropertyAccessor, resolvers);
                }
            }
            return typeMap;
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            TypeInfo typeInfo = _typeInfos.GetOrAdd(type, t => new TypeInfo(type));

            return typeInfo;
        }

        private bool MapDestinationPropertyToSource(LinkedList<MemberInfo> resolvers, TypeInfo sourceType, string nameToSearch, IMappingOptions mappingOptions)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;

            var sourceProperties = sourceType.GetPublicReadAccessors();
            var sourceNoArgMethods = sourceType.GetPublicNoArgMethods();

			MemberInfo resolver = FindTypeMember(sourceProperties, sourceNoArgMethods, nameToSearch, mappingOptions);

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

        			var valueResolver = FindTypeMember(sourceProperties, sourceNoArgMethods, snippet.First, mappingOptions);

        			if (valueResolver != null)
        			{
        				resolvers.AddLast(valueResolver);

        				foundMatch = MapDestinationPropertyToSource(resolvers, GetTypeInfo(valueResolver.GetMemberType()), snippet.Second, mappingOptions);

        				if (!foundMatch)
        				{
        					resolvers.RemoveLast();
        				}
        			}
        		}
        	}

        	return foundMatch;
        }

        private static MemberInfo FindTypeMember(IEnumerable<MemberInfo> modelProperties, IEnumerable<MethodInfo> getMethods, string nameToSearch, IMappingOptions mappingOptions)
        {
            MemberInfo pi = modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, nameToSearch));
            if (pi != null)
                return pi;

            MethodInfo mi = getMethods.FirstOrDefault(m => NameMatches(m.Name, nameToSearch));
            if (mi != null)
                return mi;

            pi = modelProperties.FirstOrDefault(prop => NameMatches(mappingOptions.SourceMemberNameTransformer(prop.Name), nameToSearch));
            if (pi != null)
                return pi;

            mi = getMethods.FirstOrDefault(m => NameMatches(mappingOptions.SourceMemberNameTransformer(m.Name), nameToSearch));
            if (mi != null)
                return mi;

            pi = modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, mappingOptions.DestinationMemberNameTransformer(nameToSearch)));
            if (pi != null)
                return pi;

            pi = getMethods.FirstOrDefault(m => NameMatches(m.Name, mappingOptions.DestinationMemberNameTransformer(nameToSearch)));
            if (pi != null)
                return pi;

            return null;
        }

        private static bool NameMatches(string memberName, string nameToMatch)
        {
            return String.Compare(memberName, nameToMatch, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i, IMappingOptions mappingOptions)
        {
            return new NameSnippet
                    {
                        First = String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter, matches.Take(i).ToArray()),
                        Second = String.Join(mappingOptions.SourceMemberNamingConvention.SeparatorCharacter, matches.Skip(i).ToArray())
                    };
        }

        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }
    }
}
