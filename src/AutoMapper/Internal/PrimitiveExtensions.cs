namespace AutoMapper.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

	public static class PrimitiveExtensions
	{
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static bool IsNullableType(this Type type)
		{
            return type.IsGenericType() && (type.GetGenericTypeDefinition().Equals(typeof (Nullable<>)));
		}

        public static Type GetTypeOfNullable(this Type type)
        {
            return type.GetGenericArguments()[0];
        }

        public static bool IsCollectionType(this Type type)
        {
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof (ICollection<>))
            {
                return true;
            }

            IEnumerable<Type> genericInterfaces = type.GetInterfaces().Where(t => t.IsGenericType());
            IEnumerable<Type> baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());
            
            var isCollectionType = baseDefinitions.Any(t => t == typeof (ICollection<>));

            return isCollectionType;
        }


		public static bool IsEnumerableType(this Type type)
		{
			return type.GetInterfaces().Contains(typeof (IEnumerable));
		}

        public static bool IsQueryableType(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(IQueryable));
        }

		public static bool IsListType(this Type type)
		{
			return type.GetInterfaces().Contains(typeof (IList));
		}

		public static bool IsListOrDictionaryType(this Type type)
		{
			return type.IsListType() || type.IsDictionaryType();
		}

		public static bool IsDictionaryType(this Type type)
		{
            if (type.IsGenericType() &&
                type.GetGenericTypeDefinition() == typeof (System.Collections.Generic.IDictionary<,>))
				return true;

            var genericInterfaces = type.GetInterfaces().Where(t => t.IsGenericType());
			var baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());
            return baseDefinitions.Any(t => t == typeof (System.Collections.Generic.IDictionary<,>));
		}

		public static Type GetDictionaryType(this Type type)
		{
            if (type.IsGenericType() &&
                type.GetGenericTypeDefinition() == typeof (System.Collections.Generic.IDictionary<,>))
				return type;

            var genericInterfaces =
                type.GetInterfaces()
                    .Where(
                        t =>
                            t.IsGenericType() &&
                            t.GetGenericTypeDefinition() == typeof (System.Collections.Generic.IDictionary<,>));
			return genericInterfaces.FirstOrDefault();
		}

        public static Type GetGenericElementType(this Type type)
        {
            if (type.HasElementType)
                return type.GetElementType();
            return type.GetGenericArguments()[0];
        }
	}
}
