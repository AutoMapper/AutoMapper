namespace AutoMapper.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class PrimitiveExtensions
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
            return type.IsGenericType(typeof(Nullable<>));
        }

        public static Type GetTypeOfNullable(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments[0];
        }

        public static bool IsCollectionType(this Type type)
        {
            return type.ImplementsGenericInterface(typeof(ICollection<>));
        }


        public static bool IsEnumerableType(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsQueryableType(this Type type)
        {
            return typeof(IQueryable).IsAssignableFrom(type);
        }

        public static bool IsListType(this Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        public static bool IsListOrDictionaryType(this Type type)
        {
            return type.IsListType() || type.IsDictionaryType();
        }

        public static bool IsDictionaryType(this Type type)
        {
            return type.ImplementsGenericInterface(typeof(IDictionary<,>));
        }

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
        {
            if(type.IsGenericType(interfaceType))
            {
                return true;
            }
            foreach(var @interface in type.GetTypeInfo().ImplementedInterfaces)
            {
                if(@interface.IsGenericType(interfaceType))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsGenericType(this Type type, Type genericType)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition() == genericType;
        }

        public static Type GetIEnumerableType(this Type type)
        {
            return type.GetGenericInterface(typeof(IEnumerable<>));
        }

        public static Type GetDictionaryType(this Type type)
        {
            return type.GetGenericInterface(typeof(IDictionary<,>));
        }

        public static Type GetGenericInterface(this Type type, Type genericInterface)
        {
            if(type.IsGenericType(genericInterface))
            {
                return type;
            }
            return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t=>t.IsGenericType(genericInterface));
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