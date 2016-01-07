namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Internal;

    public class EnumerableMapper : EnumerableMapperBase<IList>
    {
        public override bool IsMatch(TypePair context)
        {
            // destination type must be IEnumerable interface or a class implementing at least IList 
            return ((context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType()) ||
                    context.DestinationType.IsListType())
                   && context.SourceType.IsEnumerableType();
        }

        protected override void SetElementValue(IList destination, object mappedValue, int index)
        {
            destination.Add(mappedValue);
        }

        protected override void ClearEnumerable(IList enumerable)
        {
            enumerable.Clear();
        }

        protected override object GetOrCreateDestinationObject(ResolutionContext context, Type destElementType,
            int sourceLength)
        {
            if (context.DestinationValue is IList && !(context.DestinationValue is Array))
                return context.DestinationValue;

            return ObjectCreator.CreateList(destElementType);
        }

        protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            return ObjectCreator.CreateList(destElementType);
        }
    }
}