namespace AutoMapper.Mappers
{
    public class StringMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            return context.SourceValue?.ToString();
        }

        public bool IsMatch(TypePair context, IConfigurationProvider configuration)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }
    }
}