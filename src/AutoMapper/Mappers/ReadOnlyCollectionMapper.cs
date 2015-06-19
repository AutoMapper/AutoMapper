namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        private Type EnumerableMapperType { get; } = typeof (EnumerableMapper<>);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var elementType = context.DestinationType.GetNullEnumerableElementType();

            var enumerableMapper = EnumerableMapperType.MakeGenericType(elementType);

            var objectMapper = (IObjectMapper) Activator.CreateInstance(enumerableMapper);

            var nullDestinationValueSoTheReadOnlyCollectionMapperWorks =
                context.PropertyMap != null
                    ? context.CreateMemberContext(context.TypeMap, context.SourceValue, null, context.SourceType,
                        context.PropertyMap)
                    : context;

            return objectMapper.Map(nullDestinationValueSoTheReadOnlyCollectionMapperWorks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        #region Nested type: EnumerableMapper

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        private class EnumerableMapper<TElement> : EnumerableMapperBase<IList<TElement>>
        {
            /// <summary>
            /// 
            /// </summary>
            private readonly IList<TElement> _inner = new List<TElement>();

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
            /// <param name="elements"></param>
            /// <param name="mappedValue"></param>
            /// <param name="index"></param>
            protected override void SetElementValue(IList<TElement> elements, object mappedValue, int index)
            {
                _inner.Add((TElement) mappedValue);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destination"></param>
            /// <returns></returns>
            protected override IList<TElement> GetEnumerableFor(object destination)
            {
                return _inner;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destElementType"></param>
            /// <param name="sourceLength"></param>
            /// <returns></returns>
            protected override IList<TElement> CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="destinationElementType"></param>
            /// <param name="count"></param>
            /// <param name="mapper"></param>
            /// <returns></returns>
            protected override object CreateDestinationObject(ResolutionContext context, Type destinationElementType,
                int count, IMappingEngineRunner mapper)
            {
                return new ReadOnlyCollection<TElement>(_inner);
            }
        }

        #endregion
    }
}