namespace AutoMapper.Configuration
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

        public static MemberInfo GetFieldOrProperty(this Type type, string name)
        {
            var memberInfo = new[] { type }
                .RecursiveSelect(i => i.GetTypeInfo().ImplementedInterfaces)
                .Distinct()
                .Select(i => i.GetMember(name).FirstOrDefault())
                .FirstOrDefault(m => m != null);
            if(memberInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Cannot find a field or property named " + name);
            }
            return memberInfo;
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType() && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        public static Type GetTypeOfNullable(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments[0];
        }

        public static bool IsCollectionType(this Type type)
        {
            if(type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                return true;
            }

            IEnumerable<Type> genericInterfaces = type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType());
            IEnumerable<Type> baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());

            var isCollectionType = baseDefinitions.Any(t => t == typeof(ICollection<>));

            return isCollectionType;
        }


        public static bool IsEnumerableType(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IEnumerable));
        }

        public static bool IsQueryableType(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IQueryable));
        }

        public static bool IsListType(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IList));
        }

        public static bool IsListOrDictionaryType(this Type type)
        {
            return type.IsListType() || type.IsDictionaryType();
        }

        public static bool IsDictionaryType(this Type type)
        {
            if(type.IsGenericType() &&
                type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
                return true;

            var genericInterfaces = type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType());
            var baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());
            return baseDefinitions.Any(t => t == typeof(System.Collections.Generic.IDictionary<,>));
        }

        public static Type GetDictionaryType(this Type type)
        {
            if(type.IsGenericType() &&
                type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
                return type;

            var genericInterfaces =
                type.GetTypeInfo().ImplementedInterfaces
                    .Where(
                        t =>
                            t.IsGenericType() &&
                            t.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>));
            return genericInterfaces.FirstOrDefault();
        }

        public static Type GetGenericElementType(this Type type)
        {
            if(type.HasElementType)
                return type.GetElementType();
            return type.GetTypeInfo().GenericTypeArguments[0];
        }

        public static IEnumerable<TSource> RecursiveSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> childSelector)
        {
            return RecursiveSelect(source, childSelector, element => element);
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
           Func<TSource, IEnumerable<TSource>> childSelector, Func<TSource, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, (element, index, depth) => selector(element));
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
           Func<TSource, IEnumerable<TSource>> childSelector, Func<TSource, int, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, (element, index, depth) => selector(element, index));
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
           Func<TSource, IEnumerable<TSource>> childSelector, Func<TSource, int, int, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, selector, 0);
        }

        private static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
           Func<TSource, IEnumerable<TSource>> childSelector, Func<TSource, int, int, TResult> selector, int depth)
        {
            return source.SelectMany((element, index) => Enumerable.Repeat(selector(element, index, depth), 1)
               .Concat(RecursiveSelect(childSelector(element) ?? Enumerable.Empty<TSource>(),
                  childSelector, selector, depth + 1)));
        }
    }
}