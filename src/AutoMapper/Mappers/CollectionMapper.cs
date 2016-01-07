namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    public class CollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            Type genericType = typeof (EnumerableMapper<,>);

            var collectionType = context.DestinationType;
            var elementType = TypeHelper.GetElementType(context.DestinationType);

            var enumerableMapper = genericType.MakeGenericType(collectionType, elementType);

            var objectMapper = (IObjectMapper) Activator.CreateInstance(enumerableMapper);

            return objectMapper.Map(context);
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        #region Nested type: EnumerableMapper

        private class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
            where TCollection : ICollection<TElement>
        {
            public override bool IsMatch(TypePair context)
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
                Object collection;

                if (typeof (TCollection).IsInterface())
                {
                    collection = new List<TElement>();
                }
                else
                {
                    collection = ObjectCreator.CreateDefaultValue(typeof (TCollection));
                }

                return (TCollection) collection;
            }
        }

        #endregion
    }
}