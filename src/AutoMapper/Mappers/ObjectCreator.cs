namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// Instantiates objects
    /// </summary>
    public static class ObjectCreator
    {
        /// <summary>
        /// 
        /// </summary>
        private static DelegateFactory DelegateFactory { get; } = new DelegateFactory();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Array CreateArray(Type elementType, int length)
        {
            return Array.CreateInstance(elementType, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static IList CreateList(Type elementType)
        {
            Type destListType = typeof (List<>).MakeGenericType(elementType);
            return (IList) CreateObject(destListType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionaryType"></param>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static object CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            var type = dictionaryType.IsInterface()
                ? typeof (Dictionary<,>).MakeGenericType(keyType, valueType)
                : dictionaryType;

            return CreateObject(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateDefaultValue(Type type)
        {
            return type.IsValueType() ? CreateObject(type) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateNonNullValue(Type type)
        {
            return type.IsValueType()
                ? CreateObject(type)
                : type == typeof (string)
                    ? string.Empty
                    : CreateObject(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateObject(Type type)
        {
            return type.IsArray
                ? CreateArray(type.GetElementType(), 0)
                : type == typeof (string)
                    ? null
                    : DelegateFactory.CreateCtor(type)();
        }
    }
}