using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Internal;
using AutoMapper.ReflectionExtensions;

namespace AutoMapper
{
	internal class TypeMapFactory
	{
		private readonly Type _sourceType;
		private readonly Type _destinationType;

		public TypeMapFactory(Type sourceType, Type destinationType)
		{
			_sourceType = sourceType;
			_destinationType = destinationType;
		}

		public TypeMap CreateTypeMap()
		{
			var typeMap = new TypeMap(_sourceType, _destinationType);

			IMemberAccessor[] destProperties = _destinationType.GetPublicReadAccessors();

			foreach (IMemberAccessor destProperty in destProperties)
			{
				var propertyMap = new PropertyMap(destProperty);

				if (MapDestinationPropertyToSource(propertyMap, _sourceType, destProperty.Name))
				{
					typeMap.AddPropertyMap(propertyMap);
				}
			}
			return typeMap;
		}

		private static bool MapDestinationPropertyToSource(PropertyMap propertyMap, Type sourceType, string nameToSearch)
		{
			IMemberAccessor[] sourceProperties = sourceType.GetPublicReadAccessors();
			MethodInfo[] sourceNoArgMethods = sourceType.GetPublicNoArgMethods();

			IValueResolver IValueResolver = FindTypeMember(sourceProperties, sourceNoArgMethods, nameToSearch);

			bool foundMatch = IValueResolver != null;

			if (!foundMatch)
			{
				string[] matches = Regex.Matches(nameToSearch, "[A-Z][a-z0-9]*")
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

					foundMatch = MapDestinationPropertyToSource(propertyMap, valueResolver.MemberType, snippet.Second);

					if (foundMatch)
					{
						break;
					}

					propertyMap.RemoveLastResolver();
				}
			}
			else
			{
				propertyMap.ChainResolver(IValueResolver);
			}

			return foundMatch;
		}

		private static IMemberAccessor FindTypeMember(IMemberAccessor[] modelProperties, MethodInfo[] getMethods, string nameToSearch)
		{
			IMemberAccessor pi = ReflectionHelper.FindModelPropertyByName(modelProperties, nameToSearch);
			if (pi != null)
				return pi;

			MethodInfo mi = ReflectionHelper.FindModelMethodByName(getMethods, nameToSearch);
			if (mi != null)
				return new MethodAccessor(mi);

			return null;
		}

		private static NameSnippet CreateNameSnippet(string[] matches, int i)
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
