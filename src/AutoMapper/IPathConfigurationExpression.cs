using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    /// <typeparam name="TDestination">Destination type for this map</typeparam>
    /// <typeparam name="TMember">Type for this member</typeparam>
    public interface IPathConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();

        void Condition(Func<ConditionParameters<TSource, TDestination, TMember>, bool> condition);
    }

    public readonly struct ConditionParameters<TSource, TDestination, TMember>
    {
        public ConditionParameters(TSource source, TDestination destination, TMember sourceMember, TMember destinationMember, ResolutionContext context)
        {
            Source = source;
            Destination = destination;
            SourceMember = sourceMember;
            DestinationMember = destinationMember;
            Context = context;
        }
        public TSource Source { get; }
        public TDestination Destination { get; }
        public TMember SourceMember { get; }
        public TMember DestinationMember { get; }
        public ResolutionContext Context { get; }
    }
}