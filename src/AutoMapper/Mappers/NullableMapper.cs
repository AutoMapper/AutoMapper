namespace AutoMapper.Mappers
{
    using Internal;

    public class NullableMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            return context.SourceValue;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return context.DestinationType.IsNullableType();
        }
    }
}