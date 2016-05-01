using System;
using System.Collections.Generic;
using AutoMapper.Configuration;

namespace AutoMapper.Mappers
{
    public class CollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            var enumerableMapper = TypeHelper.GetEnumerableMapper(context.DestinationType);
            return enumerableMapper.Map(context);
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }
    }

    internal class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
        where TCollection : ICollection<TElement>
    {
        public override bool IsMatch(TypePair context)
        {
            throw new NotImplementedException();
        }

        protected override void SetElementValue(TCollection destination, object mappedValue, int index)
        {
            destination.Add((TElement)mappedValue);
        }

        protected override void ClearEnumerable(TCollection enumerable)
        {
            enumerable.Clear();
        }

        protected override TCollection CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            Object collection;

            if(typeof(TCollection).IsInterface())
            {
                collection = new List<TElement>();
            }
            else
            {
                collection = ObjectCreator.CreateDefaultValue(typeof(TCollection));
            }

            return (TCollection)collection;
        }
    }
}