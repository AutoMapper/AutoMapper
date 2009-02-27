using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	public class EnumerableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			IEnumerable<object> enumerableValue = ((IEnumerable)context.SourceValue).Cast<object>();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType);

			Type destElementType = TypeHelper.GetElementType(context.DestinationType);
			Type destListType = typeof(List<>).MakeGenericType(destElementType);
			IList destinationList = (IList)mapper.CreateObject(destListType);

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

				destinationList.Add(mappedValue);

				i++;
			}

			return destinationList;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return (context.DestinationType.IsEnumerableType()) && (context.SourceType.IsEnumerableType());
		}
	}
}