namespace AutoMapper.Mappers
{
    public class StringMapper<T> : IObjectMapper<T, string>
    {
        public string Map(T source, string destination, ResolutionContext context) => source?.ToString();
    }
}