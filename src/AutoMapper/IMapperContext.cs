namespace AutoMapper
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMapperContext : IMapperConfigurationContext, IMapperMappingContext
    {
        /// <summary>
        /// Resets all existing configuration.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IMapperContextFactory
    {
        /// <summary>
        /// Returns a newly created <see cref="IMapperContext"/>.
        /// </summary>
        /// <returns></returns>
        IMapperContext CreateMapperContext();
    }
}