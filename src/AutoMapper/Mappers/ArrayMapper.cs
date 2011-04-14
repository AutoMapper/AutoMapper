using System;

namespace AutoMapper.Mappers
{
	public class ArrayMapper : EnumerableMapperBase<Array>
	{
		public override bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
		}

		protected override void ClearEnumerable(Array enumerable)
		{
			// no op
		}

		protected override void SetElementValue(Array destination, object mappedValue, int index)
		{
            if(destination == null) throw new ArgumentNullException("destination");

			destination.SetValue(mappedValue, index);
		}

		protected override Array CreateDestinationObjectBase(Type destElementType, int sourceLength)
		{
            if(destElementType == null) throw new ArgumentNullException("destElementType");

			return ObjectCreator.CreateArray(destElementType, sourceLength);
		}
	}
}