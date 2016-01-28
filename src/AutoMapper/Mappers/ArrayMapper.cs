namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using Internal;

    public class ArrayMapper : EnumerableMapperBase<Array>
    {
        public override bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        protected override void ClearEnumerable(Array enumerable)
        {
            // no op
        }

        protected override void SetElementValue(Array destination, object mappedValue, int index)
        {
            destination.SetValue(mappedValue, index);
        }

        protected override Array CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldAssignEnumerable(ResolutionContext context)
        {
            return !context.IsSourceValueNull && context.DestinationType.IsAssignableFrom(context.SourceType);
        }

        protected override object GetOrCreateDestinationObject(ResolutionContext context, Type destElementType, int sourceLength)
        {
            return ObjectCreator.CreateArray(destElementType, sourceLength);
        }
    }
}