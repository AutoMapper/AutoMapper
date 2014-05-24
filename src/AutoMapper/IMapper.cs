namespace AutoMapper
{
    /// <summary>
    /// Represents a mapper with and independent configuration, for both creating maps and performing maps.
    /// </summary>
    public interface IMapper : IMappingEngine, IConfiguration
    {
    }
}
