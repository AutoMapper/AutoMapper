using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	public abstract class EnumerableMapperBase<TEnumerable> : IObjectMapper
		where TEnumerable : IEnumerable
	{
        //TODO: Update to detect and use existing values if the count of source and destination is the same
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
		    bool mapToExisting = true;

			var sourceValue = (IEnumerable)context.SourceValue ?? new object[0];
			IList<object> enumerableSourceValues = sourceValue.Cast<object>().ToList();
            
            var destinationValue = (IEnumerable)context.DestinationValue ?? new object[0];
			IList<object> enumerableDestinationValues = destinationValue.Cast<object>().ToList();

            Type sourceElementType = TypeHelper.GetElementType(context.SourceType, sourceValue);
			Type destElementType = TypeHelper.GetElementType(context.DestinationType);

            var sourceLength = enumerableSourceValues.Count;
		    var destinationLength = enumerableDestinationValues.Count;

            //Careful don't wipe out and set the destination unless you mean it
			var destination = (context.DestinationValue ?? CreateDestinationObject(context, destElementType, sourceLength, mapper));
			var enumerableDestination = GetEnumerableFor(destination);

            //Validate that incoming context has UseDestinationValue set, then if source and destination sizes are equal don't clear but map to existing items.
            if (context.PropertyMap != null && !context.PropertyMap.UseDestinationValue || sourceLength != destinationLength)
            {
                ClearEnumerable(enumerableDestination);
                mapToExisting = false;
            }

            if (mapToExisting)
            {
                int sourceCount = enumerableSourceValues.Count();
                for (int j = 0; j < sourceCount; j++)
                {
                    var item = enumerableSourceValues[j];
                    var newContext = context.CreateElementContext(null, item, sourceElementType, destElementType, j);
                    var elementResolutionResult = new ResolutionResult(newContext);

                    var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(elementResolutionResult, destElementType);

                    Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                    newContext = context.CreateMemberContext(typeMap, item, enumerableDestinationValues[j], targetSourceType, newContext.PropertyMap);
                    mapper.Map(newContext);
                }


            }
            else
            {
                int i = 0;
                foreach (object item in enumerableSourceValues)
                {
                    var newContext = context.CreateElementContext(null, item, sourceElementType, destElementType, i);
                    var elementResolutionResult = new ResolutionResult(newContext);

                    var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(elementResolutionResult, destElementType);

                    Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                    Type targetDestinationType = typeMap != null ? typeMap.DestinationType : destElementType;

                    newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, i);

                    object mappedValue = mapper.Map(newContext);

                    SetElementValue(enumerableDestination, mappedValue, i);

                    i++;
                }
            }
		    object valueToAssign = destination;
			return valueToAssign;
		}


		protected virtual TEnumerable GetEnumerableFor(object destination)
		{
			return (TEnumerable) destination;
		}

		protected virtual void ClearEnumerable(TEnumerable enumerable) { }

		private object CreateDestinationObject(ResolutionContext context, Type destinationElementType, int count, IMappingEngineRunner mapper)
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