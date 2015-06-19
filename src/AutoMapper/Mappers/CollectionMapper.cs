namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class CollectionMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        private Type EnumerableMapperType { get; } = typeof (EnumerableMapper<,>);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var collectionType = context.DestinationType;
            var elementType = context.DestinationType.GetNullEnumerableElementType();

            var enumerableMapper = EnumerableMapperType.MakeGenericType(collectionType, elementType);

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
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        #region Nested type: EnumerableMapper

        private class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
            where TCollection : ICollection<TElement>
        {
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            protected override void SetElementValue(TCollection destination, object mappedValue, int index)
            {
                destination.Add((TElement) mappedValue);
            }

            protected override void ClearEnumerable(TCollection enumerable)
            {
                enumerable.Clear();
            }

            protected override TCollection CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                var collection = typeof (TCollection).IsInterface()
                    ? new List<TElement>()
                    : ObjectCreator.CreateDefaultValue(typeof (TCollection));

                return (TCollection) collection;
            }
        }

        #endregion
    }
}