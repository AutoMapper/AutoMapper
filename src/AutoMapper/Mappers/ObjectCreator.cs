namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// Instantiates objects
    /// </summary>
    public static class ObjectCreator
    {
        private static readonly DelegateFactory DelegateFactory = new DelegateFactory();

        public static Array CreateArray(Type elementType, int length)
        {
            return Array.CreateInstance(elementType, length);
        }

        public static Array CreateArray(Type elementType, Array sourceArray)
        {
            return Array.CreateInstance(elementType, sourceArray.GetLengths());
        }

        public static IList CreateList(Type elementType)
        {
            Type destListType = typeof (List<>).MakeGenericType(elementType);
            return (IList) CreateObject(destListType);
        }

        public static object CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            var type = dictionaryType.IsInterface()
                ? typeof (Dictionary<,>).MakeGenericType(keyType, valueType)
                : dictionaryType;

            return CreateObject(type);
        }

        public static object CreateDefaultValue(Type type)
        {
            return type.IsValueType() ? CreateObject(type) : null;
        }

        public static object CreateNonNullValue(Type type)
        {
            return type.IsValueType()
                ? CreateObject(type)
                : type == typeof (string)
                    ? string.Empty
                    : CreateObject(type);
        }

        public static object CreateObject(Type type)
        {
            return type.IsArray
                ? CreateArray(type.GetElementType(), 0)
                : type == typeof (string)
                    ? null
                    : DelegateFactory.CreateCtor(type)();
        }
    }

    internal static class ArrayExtensions
    {
        public static int[] GetLengths(this Array array)
        {
            return Enumerable.Range(0, array.Rank).Select(dimension => array.GetLength(dimension)).ToArray();
        }
    }
}