using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
	internal static class ObjectCreator
	{
		public static Array CreateArray(Type elementType, int length)
		{
			return Array.CreateInstance(elementType, length);
		}

		public static IList CreateList(Type elementType)
		{
			Type destListType = typeof(List<>).MakeGenericType(elementType);
			return (IList) Activator.CreateInstance(destListType, true);
		}

		public static object CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
		{
			var type = dictionaryType.IsInterface
			           	? typeof(Dictionary<,>).MakeGenericType(keyType, valueType)
			           	: dictionaryType;

			return Activator.CreateInstance(type, true);
		}
	}
}