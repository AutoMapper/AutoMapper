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
			return (IList) CreateObject(destListType);
		}

		public static object CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
		{
			var type = dictionaryType.IsInterface
			           	? typeof(Dictionary<,>).MakeGenericType(keyType, valueType)
			           	: dictionaryType;

			return CreateObject(type);
		}

		public static object CreateDefaultValue(Type type)
		{
			return !type.IsValueType ? null : CreateObject(type);
		}

		public static object CreateObject(Type type)
		{
			return DelegateFactory.CreateCtor(type)();
		}
	}
}