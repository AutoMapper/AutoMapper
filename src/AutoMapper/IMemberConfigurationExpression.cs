using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    /// <typeparam name="TMember">Type for this member</typeparam>
    /// <typeparam name="TDestination">Destination type for this map</typeparam>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
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
        /// Map destination member using a custom value resolver
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        void MapFrom<TValueResolver>() 
            where TValueResolver : IValueResolver<TSource, TDestination, TMember>;

        /// <summary>
        /// Map destination member using a custom member value resolver supplied with a source member
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        /// <typeparam name="TSourceMember">Source member to supply</typeparam>
        void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) 
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;

        /// <summary>
        /// Map destination member using a custom member value resolver supplied from a source member name
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TValueResolver">Value resolver type</typeparam>
        /// <typeparam name="TSourceMember">Source member to supply</typeparam>
        /// <param name="sourceMemberName">Source member name</param>
        void MapFrom<TValueResolver, TSourceMember>(string sourceMemberName) 
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;

        /// <summary>
        /// Map destination member using a custom value resolver instance
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="valueResolver">Value resolver instance to use</param>
        void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver);

        /// <summary>
        /// Map destination member using a custom value resolver instance
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <param name="sourceMember">Source member to supply to value resolver</param>
        void MapFrom<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Map destination member using a custom function. Access both the source and destination object.
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="mappingFunction">Function to map to destination member</param>
        void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction);

        /// <summary>
        /// Map destination member using a custom function. Access the source, destination object, and destination member.
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="mappingFunction">Function to map to destination member</param>
        void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction);

        /// <summary>
        /// Map destination member using a custom function. Access the source, destination object, destination member, and context.
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="mappingFunction">Function to map to destination member</param>
        void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction);

        /// <summary>
        /// Map destination member using a custom expression. Used in LINQ projection (ProjectTo).
        /// </summary>
        /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
        /// <param name="mapExpression">Map expression</param>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression);

        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// </summary>
        /// <param name="sourceMemberName">Property name referencing the source member to map against</param>
        void MapFrom(string sourceMemberName);

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
        /// Conditionally map this member, evaluated before accessing the source value
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object and the current resolution context</param>
        void PreCondition(Func<TSource, ResolutionContext, bool> condition);

        /// <summary>
        /// Conditionally map this member, evaluated before accessing the source value
        /// </summary>
        /// <param name="condition">Condition to evaluate using the source object, the destination object, and the current resolution context</param>
        void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition);

        /// <summary>
        /// Ignore this member for LINQ projections unless explicitly expanded during projection
        /// </summary>
        void ExplicitExpansion();

        /// <summary>
        /// The destination member being configured.
        /// </summary>
        MemberInfo DestinationMember { get; }

        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <param name="transformer">Transformation expression</param>
        void AddTransform(Expression<Func<TMember, TMember>> transformer);

        /// <summary>
        /// Specify a value converter to convert from the matching source member to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TValueConverter">Value converter type</typeparam>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        void ConvertUsing<TValueConverter, TSourceMember>() where TValueConverter : IValueConverter<TSourceMember, TMember>;

        /// <summary>
        /// Specify a value converter to convert from the specified source member to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TValueConverter">Value converter type</typeparam>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <param name="sourceMember">Source member to supply to the value converter</param>
        void ConvertUsing<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) where TValueConverter : IValueConverter<TSourceMember, TMember>;

        /// <summary>
        /// Specify a value converter to convert from the specified source member name to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TValueConverter">Value converter type</typeparam>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
        void ConvertUsing<TValueConverter, TSourceMember>(string sourceMemberName) where TValueConverter : IValueConverter<TSourceMember, TMember>;

        /// <summary>
        /// Specify a value converter instance to convert from the matching source member to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <param name="valueConverter">Value converter instance</param>
        void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter);

        /// <summary>
        /// Specify a value converter instance from the specified source member to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <param name="valueConverter">Value converter instance</param>
        /// <param name="sourceMember">Source member to supply to the value converter</param>
        void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>
        /// Specify a value converter instance to convert from the specified source member name to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <param name="valueConverter">Value converter instance</param>
        /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
        void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, string sourceMemberName);
    }

    /// <summary>
    /// Configuration options for an individual member
    /// </summary>
    public interface IMemberConfigurationExpression : IMemberConfigurationExpression<object, object, object>
    {
        /// <summary>
        /// Map destination member using a custom value resolver. Used when the value resolver is not known at compile-time
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="valueResolverType">Value resolver type</param>
        void MapFrom(Type valueResolverType);

        /// <summary>
        /// Map destination member using a custom value resolver. Used when the value resolver is not known at compile-time
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="valueResolverType">Value resolver type</param>
        /// <param name="sourceMemberName">Member to supply to value resolver</param>
        void MapFrom(Type valueResolverType, string sourceMemberName);

        /// <summary>
        /// Map destination member using a custom value resolver instance
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="valueResolver">Value resolver instance to use</param>
        /// <param name="sourceMemberName">Source member to supply to value resolver</param>
        void MapFrom<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> valueResolver, string sourceMemberName);

        /// <summary>
        /// Specify a value converter type to convert from the matching source member to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <param name="valueConverterType">Value converter type</param>
        void ConvertUsing(Type valueConverterType);

        /// <summary>
        /// Specify a value converter type to convert from the specified source member name to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <param name="valueConverterType">Value converter type</param>
        /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
        void ConvertUsing(Type valueConverterType, string sourceMemberName);

        /// <summary>
        /// Specify a value converter instance to convert from the specified source member name to the destination member
        /// </summary>
        /// <remarks>
        /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
        /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
        /// </remarks>
        /// <typeparam name="TSourceMember">Source member type</typeparam>
        /// <typeparam name="TDestinationMember">Destination member type</typeparam>
        /// <param name="valueConverter">Value converter instance</param>
        /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
        void ConvertUsing<TSourceMember, TDestinationMember>(IValueConverter<TSourceMember, TDestinationMember> valueConverter, string sourceMemberName);
    }
}