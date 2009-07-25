using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class TypeMapFactory : ITypeMapFactory
	{
        private static IDictionary<Type, TypeInfo> _typeInfos = new Dictionary<Type, TypeInfo>();
        private object _typeInfoSync = new object();

		public TypeMap CreateTypeMap(Type sourceType, Type destinationType)
		{
		    var sourceTypeInfo = GetTypeInfo(sourceType);
		    var destTypeInfo = GetTypeInfo(destinationType);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo);

		    foreach (IMemberAccessor destProperty in destTypeInfo.GetPublicReadAccessors())
			{
				var propertyMap = new PropertyMap(destProperty);

                if (MapDestinationPropertyToSource(propertyMap, sourceTypeInfo, destProperty.Name))
				{
					typeMap.AddPropertyMap(propertyMap);
				}
			}
			return typeMap;
		}

        private TypeInfo GetTypeInfo(Type type)
        {
            TypeInfo typeInfo;

            if (!_typeInfos.TryGetValue(type, out typeInfo))
            {
                lock (_typeInfoSync)
                {
                    if (!_typeInfos.TryGetValue(type, out typeInfo))
                    {
                        typeInfo = new TypeInfo(type);
                        _typeInfos.Add(type, typeInfo);
                    }
                }
            }

            return typeInfo;
        }

		private bool MapDestinationPropertyToSource(PropertyMap propertyMap, TypeInfo sourceType, string nameToSearch)
		{
			var sourceProperties = sourceType.GetPublicReadAccessors();
			var sourceNoArgMethods = sourceType.GetPublicNoArgMethods();

			IValueResolver resolver = FindTypeMember(sourceProperties, sourceNoArgMethods, nameToSearch);

			bool foundMatch = resolver != null;

			if (!foundMatch)
			{
				string[] matches = Regex.Matches(nameToSearch, @"\p{Lu}[\p{Ll}0-9]*")
					.Cast<Match>()
					.Select(m => m.Value)
					.ToArray();

				for (int i = 0; i < matches.Length - 1; i++)
				{
					NameSnippet snippet = CreateNameSnippet(matches, i);

					IMemberAccessor valueResolver = FindTypeMember(sourceProperties, sourceNoArgMethods, snippet.First);

					if (valueResolver == null)
					{
						continue;
					}

					propertyMap.ChainResolver(valueResolver);

					foundMatch = MapDestinationPropertyToSource(propertyMap, GetTypeInfo(valueResolver.MemberType), snippet.Second);

					if (foundMatch)
					{
						break;
					}

					propertyMap.RemoveLastResolver();
				}
			}
			else
			{
				propertyMap.ChainResolver(resolver);
			}

			return foundMatch;
		}

		private static IMemberAccessor FindTypeMember(IEnumerable<IMemberAccessor> modelProperties, IEnumerable<MethodInfo> getMethods, string nameToSearch)
		{
			IMemberAccessor pi = modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, nameToSearch));
			if (pi != null)
				return pi;

		    string getName = "Get" + nameToSearch;
		    MethodInfo mi = getMethods.FirstOrDefault(m => (NameMatches(m.Name, getName)) || NameMatches(m.Name, nameToSearch));
			if (mi != null)
				return new MethodAccessor(mi);

			return null;
		}

        private static bool NameMatches(string memberName, string nameToMatch)
        {
            return String.Compare(memberName, nameToMatch, StringComparison.OrdinalIgnoreCase) == 0;
        }
        
        private static NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i)
		{
			return new NameSnippet
			       	{
			       		First = String.Concat(matches.Take(i + 1).ToArray()),
			       		Second = String.Concat(matches.Skip(i + 1).ToArray())
			       	};
		}

		private class NameSnippet
		{
			public string First { get; set; }
			public string Second { get; set; }
		}
	}
}
