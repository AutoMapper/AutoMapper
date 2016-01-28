namespace AutoMapper.Mappers
{
    public class StringMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            return context.SourceValue?.ToString();
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }
    }
}