namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Internal;

    public class EnumerableMapper : EnumerableMapperBase<IList>
    {
        public override bool IsMatch(ResolutionContext context)
        {
            // destination type must be IEnumerable interface or a class implementing at least IList 
            return context.SourceType.IsEnumerableType() && (context.DestinationType.IsListType() || DestinationIListTypedAsIEnumerable(context));
        }

        private static bool DestinationIListTypedAsIEnumerable(ResolutionContext context)
        {
            return context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType() && 
                        (context.DestinationValue == null || context.DestinationValue is IList);
        }

        protected override void SetElementValue(IList destination, object mappedValue, int index)
        {
            destination.Add(mappedValue);
        }

        protected override void ClearEnumerable(IList enumerable)
        {
            enumerable.Clear();
        }

        protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            return ObjectCreator.CreateList(destElementType);
        }
    }
}