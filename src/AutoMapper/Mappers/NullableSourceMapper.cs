namespace AutoMapper.Mappers
{
    using Internal;

    public class NullableSourceMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            return context.SourceValue ?? context.Engine.CreateObject(context);
        }

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsNullableType() && !context.DestinationType.IsNullableType();
        }
    }
}