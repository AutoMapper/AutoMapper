namespace AutoMapper
{
    using System.Linq.Expressions;
    using System.Collections.Concurrent;
    using QueryableExtensions;

    /// <summary>
    /// Main entry point for executing maps
    /// </summary>
	public interface IMappingEngineRunner
	{
		object Map(ResolutionContext context);
		object CreateObject(ResolutionContext context);
		IConfigurationProvider ConfigurationProvider { get; }
	    bool ShouldMapSourceValueAsNull(ResolutionContext context);
	    bool ShouldMapSourceCollectionAsNull(ResolutionContext context);

        Expression CreateMapExpression(ExpressionRequest request,
            Expression instanceParameter, ConcurrentDictionary<ExpressionRequest, int> typePairCount);

        LambdaExpression CreateMapExpression(ExpressionRequest request, ConcurrentDictionary<ExpressionRequest, int> typePairCount);
	}
}
