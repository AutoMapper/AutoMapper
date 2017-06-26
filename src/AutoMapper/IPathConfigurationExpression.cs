using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    /// <typeparam name="TDestination">Destination type for this map</typeparam>
    public interface IPathConfigurationExpression<TSource, out TDestination>
    {
        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
    }
}