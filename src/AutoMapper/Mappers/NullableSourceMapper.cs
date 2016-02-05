namespace AutoMapper.Mappers
{
    using Configuration;

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