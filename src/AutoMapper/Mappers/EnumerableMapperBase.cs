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
			if (context.DestinationType.IsAssignableFrom(context.SourceType) && context.SourceValue != null)
			{
				return context.SourceValue;
			}

			var sourceValue = (IEnumerable)context.SourceValue ?? new object[0];
			IEnumerable<object> enumerableValue = sourceValue.Cast<object>();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
			Type destElementType = TypeHelper.GetElementType(context.DestinationType);

			var sourceLength = enumerableValue.Count();
			TEnumerable destination = CreateDestinationObject(context.DestinationType, destElementType, sourceLength, mapper);

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

		private TEnumerable CreateDestinationObject(Type destinationType, Type destinationElementType, int count, IMappingEngineRunner mapper)
		{
			if (!destinationType.IsInterface && !destinationType.IsArray)
			{
				return (TEnumerable) mapper.CreateObject(destinationType);
			}
			return CreateDestinationObjectBase(destinationElementType, count);
		}

		public abstract bool IsMatch(ResolutionContext context);
		
		protected abstract void SetElementValue(TEnumerable destination, object mappedValue, int index);
		protected abstract TEnumerable CreateDestinationObjectBase(Type destElementType, int sourceLength);
	}
}