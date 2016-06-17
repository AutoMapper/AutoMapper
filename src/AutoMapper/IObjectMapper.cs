using System.Linq.Expressions;

namespace AutoMapper
{
    /// <summary>
    /// Mapping execution strategy, as a chain of responsibility
    /// </summary>
	public interface IObjectMapper
	{
        /// <summary>
        /// Performs a map
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Mapped object</returns>
        //object Map(ResolutionContext context);

        /// <summary>
        /// When true, the mapping engine will use this mapper as the strategy
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Is match</returns>
        bool IsMatch(TypePair context);
	}

    /// <summary>
    /// Map expression strategy, based on base mapper
    /// </summary>
    public interface IObjectMapExpression : IObjectMapper
    {
        /// <summary>
        /// Builds a mapping expression equivalent to the base Map method
        /// </summary>
        /// <param name="typeMapRegistry"></param>
        /// <param name="configurationProvider"></param>
        /// <param name="propertyMap"></param>
        /// <param name="sourceExpression">Source parameter</param>
        /// <param name="destExpression">Destination parameter</param>
        /// <param name="contextExpression">ResulotionContext parameter</param>
        /// <returns>Map expression</returns>
        Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression);
    }
}
