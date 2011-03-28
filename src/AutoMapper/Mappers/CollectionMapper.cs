using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
    public class CollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            Type genericType = typeof(EnumerableMapper<,>);

            var collectionType = context.DestinationType;
            var elementType = TypeHelper.GetElementType(context.DestinationType);
            
            var enumerableMapper = genericType.MakeGenericType(collectionType, elementType);

            var objectMapper = (IObjectMapper)Activator.CreateInstance(enumerableMapper);

            return objectMapper.Map(context, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        #region Nested type: EnumerableMapper

        private class EnumerableMapper<TCollection, TElement> : EnumerableMapperBase<TCollection>
            where TCollection : IList<TElement>
        {
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            protected override void SetElementValue(TCollection destination, object mappedValue, int index)
            {
                if (destination.Count < index)
                {
                    destination.Add((TElement) mappedValue);
                }
                else
                {
                    destination[index] = (TElement) mappedValue;
                }
            }

            protected override void ClearEnumerable(TCollection enumerable)
            {
                enumerable.Clear();
            }

            protected override TCollection CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                var list = typeof(TCollection).IsInterface 
                                  ? new List<TElement>() 
                                  : ObjectCreator.CreateDefaultValue(typeof (TCollection));

                return (TCollection) list;
            }
        }

        #endregion
    }
}