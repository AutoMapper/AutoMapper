using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	public abstract class EnumerableMapperBase<TEnumerable> : IObjectMapper
		where TEnumerable : IEnumerable
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			IEnumerable<object> enumerableValue = ((IEnumerable)context.SourceValue).Cast<object>();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
			Type destElementType = TypeHelper.GetElementType(context.DestinationType);

			var sourceLength = enumerableValue.Count();
			TEnumerable destination = CreateDestinationObject(destElementType, sourceLength, mapper);

			int i = 0;
			foreach (object item in enumerableValue)
			{
				Type targetSourceType = sourceElementType;
				Type targetDestinationType = destElementType;

				if (item.GetType() != sourceElementType)
				{
				    var potentialSourceType = item.GetType();

					TypeMap itemTypeMap =
						mapper.ConfigurationProvider.FindTypeMapFor(sourceElementType, destElementType)
                        ?? mapper.ConfigurationProvider.FindTypeMapFor(potentialSourceType, destElementType);

                    var potentialDestType = itemTypeMap.GetDerivedTypeFor(potentialSourceType);

                    targetSourceType = potentialDestType != destElementType ? potentialSourceType : itemTypeMap.SourceType;
				    targetDestinationType = potentialDestType;
				}

				TypeMap derivedTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(targetSourceType, targetDestinationType);

				var newContext = context.CreateElementContext(derivedTypeMap, item, targetSourceType, targetDestinationType, i);

				object mappedValue = mapper.Map(newContext);

				SetElementValue(destination, mappedValue, i);

				i++;
			}

			object valueToAssign = destination;
			return valueToAssign;
		}

		public abstract bool IsMatch(ResolutionContext context);
		
		protected abstract void SetElementValue(TEnumerable destination, object mappedValue, int index);
		protected abstract TEnumerable CreateDestinationObject(Type destElementType, int sourceLength, IMappingEngineRunner mapper);
	}
}