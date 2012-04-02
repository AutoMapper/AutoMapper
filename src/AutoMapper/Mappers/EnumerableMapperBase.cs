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
            if (context.IsSourceValueNull && mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

			ICollection<object> enumerableValue = ((IEnumerable) context.SourceValue ?? new object[0])
				.Cast<object>()
				.ToList();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType, enumerableValue);
			Type destElementType = TypeHelper.GetElementType(context.DestinationType);

			var sourceLength = enumerableValue.Count;
			var destination = (context.DestinationValue ?? CreateDestinationObject(context, destElementType, sourceLength, mapper));
			var enumerable = GetEnumerableFor(destination);

			ClearEnumerable(enumerable);

			int i = 0;
			foreach (object item in enumerableValue)
			{
				var newContext = context.CreateElementContext(null, item, sourceElementType, destElementType, i);
				var elementResolutionResult = new ResolutionResult(newContext);

				var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(elementResolutionResult, destElementType);

				Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                Type targetDestinationType = typeMap != null ? typeMap.DestinationType : destElementType;

				newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, i);

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

		protected virtual void ClearEnumerable(TEnumerable enumerable) { }

		protected virtual object CreateDestinationObject(ResolutionContext context, Type destinationElementType, int count, IMappingEngineRunner mapper)
		{
			var destinationType = context.DestinationType;

			if (!destinationType.IsInterface && !destinationType.IsArray)
			{
				return mapper.CreateObject(context);
			}
			return CreateDestinationObjectBase(destinationElementType, count);
		}

		public abstract bool IsMatch(ResolutionContext context);


		protected abstract void SetElementValue(TEnumerable destination, object mappedValue, int index);
		protected abstract TEnumerable CreateDestinationObjectBase(Type destElementType, int sourceLength);
	}
}