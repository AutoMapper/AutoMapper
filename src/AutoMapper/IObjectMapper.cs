using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using static Expression;
    using static ExpressionExtensions;

    /// <summary>
    /// Mapping execution strategy, as a chain of responsibility
    /// </summary>
    public interface IObjectMapper
    {
        /// <summary>
        /// When true, the mapping engine will use this mapper as the strategy
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Is match</returns>
        bool IsMatch(TypePair context);

        /// <summary>
        /// Builds a mapping expression equivalent to the base Map method
        /// </summary>
        /// <param name="configurationProvider"></param>
        /// <param name="profileMap"></param>
        /// <param name="propertyMap"></param>
        /// <param name="sourceExpression">Source parameter</param>
        /// <param name="destExpression">Destination parameter</param>
        /// <param name="contextExpression">ResulotionContext parameter</param>
        /// <returns>Map expression</returns>
        Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression);
    }

    /// <summary>
    /// Base class for simple object mappers that don't want to use expressions.
    /// </summary>
    /// <typeparam name="TSource">type of the source</typeparam>
    /// <typeparam name="TDestination">type of the destination</typeparam>
    public abstract class ObjectMapper<TSource, TDestination> : IObjectMapper
    {
        private static readonly MethodInfo MapMethod = typeof(ObjectMapper<TSource, TDestination>).GetDeclaredMethod("Map");

        /// <summary>
        /// When true, the mapping engine will use this mapper as the strategy
        /// </summary>
        /// <param name="context">Resolution context</param>
        /// <returns>Is match</returns>
        public abstract bool IsMatch(TypePair context);

        /// <summary>
        /// Performs conversion from source to destination type
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <param name="context">Resolution context</param>
        /// <returns>Destination object</returns>
        public abstract TDestination Map(TSource source, TDestination destination, ResolutionContext context);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(Constant(this), MapMethod, ToType(sourceExpression, typeof(TSource)), ToType(destExpression, typeof(TDestination)), contextExpression);
        }
    }
}