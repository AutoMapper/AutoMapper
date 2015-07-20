#if MONODROID || MONOTOUCH || __IOS__ || NET4
namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class ListSourceMapper : EnumerableMapperBase<IList>
    {
        public override bool IsMatch(ResolutionContext context)
        {
            return (typeof (IListSource).IsAssignableFrom(context.DestinationType));
        }

        protected override void SetElementValue(IList destination, object mappedValue, int index)
        {
            destination.Add(mappedValue);
        }

        protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            throw new NotImplementedException();
        }

        protected override IList GetEnumerableFor(object destination)
        {
            var listSource = (IListSource) destination;
            return listSource.GetList();
        }

        protected override void ClearEnumerable(IList enumerable)
        {
            enumerable.Clear();
        }
    }
}

#endif