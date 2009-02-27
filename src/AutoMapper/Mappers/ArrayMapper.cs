using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	public class ArrayMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			IEnumerable<object> enumerableValue = ((IEnumerable)context.SourceValue).Cast<object>();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
			Type destElementType = context.DestinationType.GetElementType();

			Array destArray = Array.CreateInstance(destElementType, enumerableValue.Count());

			int i = 0;
			foreach (object item in enumerableValue)
			{
				Type targetSourceType = sourceElementType;
				Type targetDestinationType = destElementType;

				if (item.GetType() != sourceElementType)
				{
					targetSourceType = item.GetType();

					TypeMap itemTypeMap =
						mapper.Configuration.FindTypeMapFor(sourceElementType, destElementType)
						?? mapper.Configuration.FindTypeMapFor(targetSourceType, destElementType);

					targetDestinationType = itemTypeMap.GetDerivedTypeFor(targetSourceType);
				}

				TypeMap derivedTypeMap = mapper.Configuration.FindTypeMapFor(targetSourceType, targetDestinationType);

				var newContext = context.CreateElementContext(derivedTypeMap, item, targetSourceType, targetDestinationType, i);

				object mappedValue = mapper.Map(newContext);

				destArray.SetValue(mappedValue, i);

				i++;
			}

			object valueToAssign = destArray;
			return valueToAssign;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
		}
	}
}