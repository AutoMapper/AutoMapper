using System;
using System.Collections;

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

		protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
		{
			return ObjectCreator.CreateList(destElementType);
		}
	}
}