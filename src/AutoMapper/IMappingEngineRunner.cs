namespace AutoMapper
{
    /// <summary>
    /// Main entry point for executing maps
    /// </summary>
	public interface IMappingEngineRunner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="parentContext"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
		object Map(ResolutionContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        object CreateObject(ResolutionContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
	    bool ShouldMapSourceValueAsNull(ResolutionContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
	    bool ShouldMapSourceCollectionAsNull(ResolutionContext context);
	}
}
