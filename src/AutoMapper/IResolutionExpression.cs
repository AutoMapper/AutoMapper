namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Custom resolver options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    public interface IResolutionExpression<TSource> : IResolutionExpression
    {
        /// <summary>
        /// Use the specified member as the input to the resolver instead of the root <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="sourceMember">Expression for the source member</param>
        void FromMember(Expression<Func<TSource, object>> sourceMember);
    }

    /// <summary>
    /// Custom resolver options
    /// </summary>
    public interface IResolutionExpression
    {
        /// <summary>
        /// Use the supplied member as the input to the resolver instead of the root source object
        /// </summary>
        /// <param name="sourcePropertyName">Property name to use</param>
        void FromMember(string sourcePropertyName);
    }
}