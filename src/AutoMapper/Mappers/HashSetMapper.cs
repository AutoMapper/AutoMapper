namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class HashSetMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var genericType = typeof (EnumerableMapper<,>);

            var collectionType = context.DestinationType;
            var elementType = context.DestinationType.GetNullEnumerableElementType();

            var enumerableMapper = genericType.MakeGenericType(collectionType, elementType);

            var objectMapper = (IObjectMapper) Activator.CreateInstance(enumerableMapper);

            return objectMapper.Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

            return isMatch;
        }

#if !NETFX_CORE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSetType(Type type)
        {
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof (ISet<>))
            {
                return true;
            }

            var genericInterfaces = type.GetInterfaces().Where(t => t.IsGenericType());
            var baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());

            var isCollectionType = baseDefinitions.Any(t => t == typeof (ISet<>));

            return isCollectionType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        private class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
            where TCollection : ISet<TElement>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destination"></param>
            /// <param name="mappedValue"></param>
            /// <param name="index"></param>
            protected override void SetElementValue(TCollection destination, object mappedValue, int index)
            {
                destination.Add((TElement) mappedValue);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="enumerable"></param>
            protected override void ClearEnumerable(TCollection enumerable)
            {
                enumerable.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destElementType"></param>
            /// <param name="sourceLength"></param>
            /// <returns></returns>
            protected override TCollection CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                var collection = typeof (TCollection).IsInterface()
                    ? new HashSet<TElement>()
                    : ObjectCreator.CreateDefaultValue(typeof (TCollection));

                return (TCollection) collection;
            }
        }

#else
        //TODO: check these build... and run...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSetType(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>))
            {
                return true;
            }

            var genericInterfaces = type.GetTypeInfo().ImplementedInterfaces.Where(t => t.GetTypeInfo().IsGenericType);
            var baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());

            var isCollectionType = baseDefinitions.Any(t => t == typeof(ISet<>));

            return isCollectionType;
        }

        /// <summary>
        /// 
        /// </summary>
        private class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
            where TCollection : ISet<TElement>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destination"></param>
            /// <param name="mappedValue"></param>
            /// <param name="index"></param>
            protected override void SetElementValue(TCollection destination, object mappedValue, int index)
            {
                destination.Add((TElement)mappedValue);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="enumerable"></param>
            protected override void ClearEnumerable(TCollection enumerable)
            {
                enumerable.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destElementType"></param>
            /// <param name="sourceLength"></param>
            /// <returns></returns>
            protected override TCollection CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                var collection = typeof(TCollection).GetTypeInfo().IsInterface
                    ? new HashSet<TElement>()
                    : ObjectCreator.CreateDefaultValue(typeof(TCollection));

                return (TCollection) collection;
            }
        }

#endif
    }
}