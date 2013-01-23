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

            var nullDestinationValueSoTheReadOnlyCollectionMapperWorks = context.CreateMemberContext(context.TypeMap, context.SourceValue, null, context.SourceType, context.PropertyMap);

            return objectMapper.Map(nullDestinationValueSoTheReadOnlyCollectionMapperWorks, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
			  if(!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType))
              return false;

			  var genericType= context.DestinationType.GetGenericTypeDefinition();
			  if (genericType == typeof(ReadOnlyCollection<>) ||
				  genericType == typeof(IReadOnlyList<>) ||
				  genericType == typeof(IReadOnlyCollection<>))
				  return true;

			  return false;
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
