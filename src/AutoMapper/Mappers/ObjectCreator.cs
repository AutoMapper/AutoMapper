namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Execution;

    /// <summary>
    /// Instantiates objects
    /// </summary>
    public static class ObjectCreator
    {
        public static readonly DelegateFactory DelegateFactory = new DelegateFactory();

        private static object CreateDictionary(Type dictionaryType)
        {
            Type keyType = dictionaryType.GetTypeInfo().GenericTypeArguments[0];
            Type valueType = dictionaryType.GetTypeInfo().GenericTypeArguments[1];
            var type = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return DelegateFactory.CreateCtor(type)();
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
                ? Array.CreateInstance(type.GetElementType(), 0)
                : type == typeof (string)
                    ? null
                    : type.IsInterface() && type.IsDictionaryType()
                        ? CreateDictionary(type) 
                        : DelegateFactory.CreateCtor(type)();
        }
    }

    internal static class ArrayExtensions
    {
        public static int[] GetLengths(this Array array)
        {
            return Enumerable.Range(0, array.Rank).Select(array.GetLength).ToArray();
        }
    }
}