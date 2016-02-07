namespace AutoMapper
{
    /// <summary>
    /// Performs mapping based on configuration
    /// </summary>
    public interface IMappingEngine
    {
        bool ShouldMapSourceValueAsNull(ResolutionContext context);
        bool ShouldMapSourceCollectionAsNull(ResolutionContext context);
        object CreateObject(ResolutionContext context);
        object Map(ResolutionContext context);
        IConfigurationProvider ConfigurationProvider { get; }
        IMapper Mapper { get; }
    }

}