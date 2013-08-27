using System;
using System.Collections;
using System.Collections.Generic;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    /// <summary>
    /// Instantiates objects
    /// </summary>
    public static class ObjectCreator
    {
        private static readonly IDelegateFactory DelegateFactory = PlatformAdapter.Resolve<IDelegateFactory>();

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
			return type.IsValueType ? CreateObject(type) : null;
		}

		public static object CreateNonNullValue(Type type)
		{
            if (type.IsValueType)
                return CreateObject(type);

            if (type == typeof(string))
                return string.Empty;

			return Activator.CreateInstance(type);
		}

		public static object CreateObject(Type type)
		{
            return type.IsArray ? CreateArray(type.GetElementType(), 0) : DelegateFactory.CreateCtor(type)();
		}
	}
}
