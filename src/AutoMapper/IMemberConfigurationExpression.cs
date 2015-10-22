namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    public interface IMemberConfigurationExpression<TSource>
    {
        /// <summary>
        /// Substitute a custom value when the source member resolves as null
        /// </summary>
        /// <param name="nullSubstitute">Value to use</param>
        void NullSubstitute(object nullSubstitute);

        /// <summary>
        /// Resolve destination member using a custom value resolver
        /// </summary>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        /// <returns>Value resolver configuration options</returns>
        IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver;

        /// <summary>
        /// Resolve destination member using a custom value resolver. Used when the value resolver is not known at compile-time
        /// </summary>
        /// <param name="valueResolverType">Value resolver type</param>
        /// <returns>Value resolver configuration options</returns>
        IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType);

        /// <summary>
        /// Resolve destination member using a custom value resolver instance
        /// </summary>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <returns>Resolution expression</returns>
        IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing(Func<TSource, object> resolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// Access both the source object and current resolution context for additional mapping, context items and parent objects
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing(Func<ResolutionResult, object> resolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// Access both the source object and current resolution context for additional mapping, context items and parent objects
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing(Func<ResolutionResult, TSource, object> resolver);

        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);

        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TMember">Member type of the source member to use</typeparam>
        /// <param name="property">Propertyname referencing the source member to map against</param>
        void MapFrom<TMember>(string property);

        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();

        /// <summary>
        /// Supply a custom mapping order instead of what the .NET runtime returns
        /// </summary>
        /// <param name="mappingOrder">Mapping order value</param>
        void SetMappingOrder(int mappingOrder);

        /// <summary>
        /// Use the destination value instead of mapping from the source value or creating a new instance
        /// </summary>
        void UseDestinationValue();

        /// <summary>
        /// Do not use the destination value instead of mapping from the source value or creating a new instance
        /// </summary>        
        void DoNotUseDestinationValue();
        
        /// <summary>
        /// Use a custom value
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="value">Value to use</param>
        void UseValue<TValue>(TValue value);

        /// <summary>
        /// Use a custom value
        /// </summary>
        /// <param name="value">Value to use</param>
        void UseValue(object value);

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, bool> condition);

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the current resolution context</param>
        void Condition(Func<ResolutionContext, bool> condition);
       
        /// <summary>
        /// Conditionally map this member, evaluated before accessing the source value
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void PreCondition(Func<TSource, bool> condition);

        /// <summary>
        /// Conditionally map this member, evaluated before accessing the source value
        /// </summary>
        /// <param name="condition">Condition to evaluate using the current resolution context</param>
        void PreCondition(Func<ResolutionContext, bool> condition);

        /// <summary>
        /// Ignore this member for LINQ projections unless explicitly expanded during projection
        /// </summary>
        void ExplicitExpansion();
    }

    /// <summary>
    /// Configuration options for an individual member
    /// </summary>
    public interface IMemberConfigurationExpression : IMemberConfigurationExpression<object>
    {
        /// <summary>
        /// Map from a specific source member
        /// </summary>
        /// <param name="sourceMember">Source member to map from</param>
        void MapFrom(string sourceMember);
    }
}