namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;

    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    /// <typeparam name="TMember">Type for this member</typeparam>
    /// <typeparam name="TDestination">Destination type for this map</typeparam>
    public interface IMemberConfigurationExpression<TSource, out TDestination, TMember>
    {
        /// <summary>
        /// Do not precompute the execution plan for this member, just map it at runtime.
        /// Simplifies the execution plan by not inlining.
        /// </summary>
        void MapAtRuntime();

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
        void ResolveUsing<TValueResolver>() 
            where TValueResolver : IValueResolver<TSource, TDestination, TMember>;

        /// <summary>
        /// Resolve destination member using a custom value resolver from a source member
        /// </summary>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        /// <typeparam name="TSourceMember">Source member to supply</typeparam>
        /// <returns>Value resolver configuration options</returns>
        void ResolveUsing<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) 
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;

        /// <summary>
        /// Resolve destination member using a custom value resolver from a source member
        /// </summary>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        /// <typeparam name="TSourceMember">Source member to supply</typeparam>
        /// <param name="sourceMemberName">Source member name</param>
        /// <returns>Value resolver configuration options</returns>
        void ResolveUsing<TValueResolver, TSourceMember>(string sourceMemberName) 
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;

        /// <summary>
        /// Resolve destination member using a custom value resolver instance
        /// </summary>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <returns>Resolution expression</returns>
        void ResolveUsing(IValueResolver<TSource, TDestination, TMember> valueResolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver instance
        /// </summary>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <param name="sourceMember">Source member to supply to value resolver</param>
        /// <returns>Resolution expression</returns>
        void ResolveUsing<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing<TResult>(Func<TSource, TResult> resolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// Access both the source object and destination member for additional mapping, context items
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing<TResult>(Func<TSource, TDestination, TResult> resolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// Access both the source object and destination member for additional mapping, context items
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, TResult> resolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. Used instead of MapFrom when not simply redirecting a source member
        /// Access both the source object and current resolution context for additional mapping, context items
        /// This method cannot be used in conjunction with LINQ query projection
        /// </summary>
        /// <param name="resolver">Callback function to resolve against source type</param>
        void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> resolver);

        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <param name="property">Propertyname referencing the source member to map against</param>
        void MapFrom(string property);

        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();

        /// <summary>
        /// Allow this member to be null. This prevents generating a check condition for it.
        /// </summary>
        void AllowNull();

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
        /// Use a custom value
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="value">Value to use</param>
        void UseValue<TValue>(TValue value);

        /// <summary>
        /// Conditionally map this member against the source, destination, source and destination members
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition);

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition);

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, TDestination, TMember, bool> condition);
       
        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, TDestination, bool> condition);
       
        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object</param>
        void Condition(Func<TSource, bool> condition);
       
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

        /// <summary>
        /// The destination member being configured.
        /// </summary>
        MemberInfo DestinationMember { get; }
    }

    /// <summary>
    /// Configuration options for an individual member
    /// </summary>
    public interface IMemberConfigurationExpression : IMemberConfigurationExpression<object, object, object>
    {
        /// <summary>
        /// Resolve destination member using a custom value resolver. Used when the value resolver is not known at compile-time
        /// </summary>
        /// <param name="valueResolverType">Value resolver type</param>
        /// <returns>Value resolver configuration options</returns>
        void ResolveUsing(Type valueResolverType);

        /// <summary>
        /// Resolve destination member using a custom value resolver. Used when the value resolver is not known at compile-time
        /// </summary>
        /// <param name="valueResolverType">Value resolver type</param>
        /// <param name="memberName">Member to supply to value resolver</param>
        /// <returns>Value resolver configuration options</returns>
        void ResolveUsing(Type valueResolverType, string memberName);

        /// <summary>
        /// Resolve destination member using a custom value resolver instance
        /// </summary>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <param name="memberName">Source member to supply to value resolver</param>
        /// <returns>Resolution expression</returns>
        void ResolveUsing<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> valueResolver, string memberName);
    }
}