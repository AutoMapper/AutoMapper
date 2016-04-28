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
        object Map(ResolutionContext context);

        /// <summary>
        /// When true, the mapping engine will use this mapper as the strategy
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Is match</returns>
        bool IsMatch(TypePair context);
	}

    public interface IObjectMapExpression
    {
        Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression);
    }
}
