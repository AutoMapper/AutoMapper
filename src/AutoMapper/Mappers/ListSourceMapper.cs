using System;
using System.Collections;
using System.ComponentModel;

namespace AutoMapper.Mappers
{
	public class ListSourceMapper : EnumerableMapperBase<IList>
	{
		public override bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return (typeof(IListSource).IsAssignableFrom(context.DestinationType));
		}

		protected override void SetElementValue(IList destination, object mappedValue, int index)
		{
            if(destination == null) throw new ArgumentNullException("destination");

			destination.Add(mappedValue);
		}

		protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
		{
			throw new NotImplementedException();
		}

		protected override IList GetEnumerableFor(object destination)
		{
            if (destination == null) throw new ArgumentNullException("destination");

			var listSource = (IListSource)destination;
			return listSource.GetList();
		}

		protected override void ClearEnumerable(IList enumerable)
		{
            if (enumerable == null) throw new ArgumentNullException("enumerable");

			enumerable.Clear();
		}
	}
}