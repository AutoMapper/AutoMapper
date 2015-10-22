namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Custom resolver options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    public interface IResolverConfigurationExpression<TSource> : IResolutionExpression<TSource>
    {
        /// <summary>
        /// Construct the value resolver with the supplied constructor function
        /// </summary>
        /// <param name="constructor">Value resolver constructor function</param>
        /// <returns>Itself</returns>
        IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor);
    }

    /// <summary>
    /// Custom resolver options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TValueResolver">Value resolver type</typeparam>
    public interface IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        /// <summary>
        /// Use the specified member as the input to the resolver instead of the root <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="sourceMember">Expression for the source member</param>
        /// <returns>Itself</returns>
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Expression<Func<TSource, object>> sourceMember);

        /// <summary>
        /// Use the specified member as the input to the resolver instead of the root <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="sourcePropertyName">Name of the source member</param>
        /// <returns>Itself</returns>
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName);

        /// <summary>
        /// Construct the value resolver with the supplied constructor function
        /// </summary>
        /// <param name="constructor">Value resolver constructor function</param>
        /// <returns>Itself</returns>
        IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor);
    }

    /// <summary>
    /// Custom resolver options
    /// </summary>
    public interface IResolverConfigurationExpression : IResolutionExpression
    {
        /// <summary>
        /// Construct the value resolver using supplied constructor function
        /// </summary>
        /// <param name="constructor">Value resolver constructor function</param>
        /// <returns>Itself</returns>
        IResolutionExpression ConstructedBy(Func<IValueResolver> constructor);
    }
}