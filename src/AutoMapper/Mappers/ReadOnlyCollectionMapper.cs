using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoMapper.Mappers
{
    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            Type genericType = typeof(EnumerableMapper<>);

            var elementType = TypeHelper.GetElementType(context.DestinationType);
            
            var enumerableMapper = genericType.MakeGenericType(elementType);

            var objectMapper = (IObjectMapper)Activator.CreateInstance(enumerableMapper);

            return objectMapper.Map(context, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
            var isMatch = context.SourceType.IsEnumerableType() &&
                          context.DestinationType.IsGenericType &&
                          context.DestinationType.GetGenericTypeDefinition() == typeof (ReadOnlyCollection<>);

            return isMatch;
        }

        #region Nested type: EnumerableMapper

        private class EnumerableMapper<TElement> : EnumerableMapperBase<IList<TElement>>
        {
            private readonly IList<TElement> inner = new List<TElement>();
            
            public override bool IsMatch(ResolutionContext context)
            {
                throw new NotImplementedException();
            }

            protected override void SetElementValue(IList<TElement> elements, object mappedValue, int index)
            {
                inner.Add((TElement)mappedValue);
            }

            protected override IList<TElement> GetEnumerableFor(object destination)
            {
                return inner;
            }

            protected override IList<TElement> CreateDestinationObjectBase(Type destElementType, int sourceLength)
            {
                throw new NotImplementedException();
            }

            protected override object CreateDestinationObject(ResolutionContext context, Type destinationElementType, int count, IMappingEngineRunner mapper)
            {
                return new ReadOnlyCollection<TElement>(inner);
            }
        }

        #endregion
    }
}
