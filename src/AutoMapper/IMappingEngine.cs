namespace AutoMapper
{
    /// <summary>
    /// Performs mapping based on configuration
    /// </summary>
    public interface IMappingEngine
    {
        object Map(ResolutionContext context);
        IConfigurationProvider ConfigurationProvider { get; }
    }
}