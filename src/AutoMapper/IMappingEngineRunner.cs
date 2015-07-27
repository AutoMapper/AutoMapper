namespace AutoMapper
{
    using System.Linq.Expressions;
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
            Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount);

        LambdaExpression CreateMapExpression(ExpressionRequest request, Internal.IDictionary<ExpressionRequest, int> typePairCount);
	}
}
