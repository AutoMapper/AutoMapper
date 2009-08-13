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
			var destination = (context.DestinationValue ?? CreateDestinationObject(context.DestinationType, destElementType, sourceLength, mapper));
			var enumerable = GetEnumerableFor(destination);

			int i = 0;
			foreach (object item in enumerableValue)
			{
			    var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(item, sourceElementType, destElementType);

                Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                Type targetDestinationType = typeMap != null ? typeMap.DestinationType : destElementType;

				var newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, i);

				object mappedValue = mapper.Map(newContext);

				SetElementValue(enumerable, mappedValue, i);

				i++;
			}

			object valueToAssign = destination;
			return valueToAssign;
		}

		protected virtual TEnumerable GetEnumerableFor(object destination)
		{
			return (TEnumerable) destination;
		}

		private object CreateDestinationObject(Type destinationType, Type destinationElementType, int count, IMappingEngineRunner mapper)
		{
			if (!destinationType.IsInterface && !destinationType.IsArray)
			{
				return mapper.CreateObject(destinationType);
			}
			return CreateDestinationObjectBase(destinationElementType, count);
		}

		public abstract bool IsMatch(ResolutionContext context);
		
		protected abstract void SetElementValue(TEnumerable destination, object mappedValue, int index);
		protected abstract TEnumerable CreateDestinationObjectBase(Type destElementType, int sourceLength);
	}
}