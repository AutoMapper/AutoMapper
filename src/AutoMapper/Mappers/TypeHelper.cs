namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <returns></returns>
        /// <remarks>Avoid confusion with the <see cref="Type.GetElementType"/> function.</remarks>
        public static Type GetNullEnumerableElementType(this Type enumerableType)
        {
            return GetElementType(enumerableType, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Type GetElementType(this Type enumerableType, IEnumerable enumerable)
        {
            if (enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }

            if (enumerableType.IsGenericType() &&
                enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                return enumerableType.GetGenericArguments()[0];
            }

            Type ienumerableType = GetIEnumerableType(enumerableType);
            if (ienumerableType != null)
            {
                return ienumerableType.GetGenericArguments()[0];
            }

            if (typeof (IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return first?.GetType() ?? typeof (object);
            }

            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.", nameof(enumerableType));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Type GetEnumerationType(this Type enumType)
        {
            if (enumType.IsNullableType())
            {
                enumType = enumType.GetGenericArguments()[0];
            }

            if (!enumType.IsEnum())
                return null;

            return enumType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <returns></returns>
        private static Type GetIEnumerableType(this Type enumerableType)
        {
            try
            {
                return enumerableType.GetInterfaces().FirstOrDefault(t => t.Name == "IEnumerable`1");
            }
            catch (AmbiguousMatchException)
            {
                if (enumerableType.BaseType() != typeof (object))
                    return GetIEnumerableType(enumerableType.BaseType());

                return null;
            }
        }
    }
}