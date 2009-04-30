using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	public class EnumerableMapper : EnumerableMapperBase<IList>
	{
		public override bool IsMatch(ResolutionContext context)
		{
			return (context.DestinationType.IsEnumerableType()) && (context.SourceType.IsEnumerableType());
		}

		protected override void SetElementValue(IList destination, object mappedValue, int index)
		{
			destination.Add(mappedValue);
		}

		protected override IList CreateDestinationObject(Type destElementType, int sourceLength, IMappingEngineRunner mapper)
		{
			Type destListType = typeof(List<>).MakeGenericType(destElementType);
			return (IList)mapper.CreateObject(destListType);
		}
	}
}