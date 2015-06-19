namespace AutoMapper
{
    /// <summary>
    /// Mapping execution strategy, as a chain of responsibility
    /// </summary>
	public interface IObjectMapper
	{
        //TODO: MappingEngineRunner needs to know about a context as well? ... in order to get at the ConfigurationProvider ...
        /// <summary>
        /// Performs a map
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Mapped object</returns>
		object Map(ResolutionContext context);
        ///// <param name="mapper">Mapping engine runner</param>
		//object Map(ResolutionContext context, IMappingEngineRunner mapper);

        /// <summary>
        /// When true, the mapping engine will use this mapper as the strategy
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Is match</returns>
		bool IsMatch(ResolutionContext context);
	}
}
