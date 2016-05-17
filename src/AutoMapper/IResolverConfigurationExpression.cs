namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Custom resolver options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    public interface IResolverConfigurationExpression<TSource>
    {
        /// <summary>
        /// Use the specified member as the input to the resolver instead of the root <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="sourceMember">Expression for the source member</param>
        /// <returns>Itself</returns>
        void FromMember(Expression<Func<TSource, object>> sourceMember);

        /// <summary>
        /// Use the specified member as the input to the resolver instead of the root <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="sourcePropertyName">Name of the source member</param>
        /// <returns>Itself</returns>
        void FromMember(string sourcePropertyName);
    }
}