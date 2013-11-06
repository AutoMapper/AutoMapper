using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    /// <summary>
    /// Mapping configuration options for non-generic maps
    /// </summary>
    public interface IMappingExpression
    {
        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// </summary>
        /// <typeparam name="TTypeConverter">Type converter type</typeparam>
        void ConvertUsing<TTypeConverter>();

        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// Use this method if you need to specify the converter type at runtime
        /// </summary>
        /// <param name="typeConverterType">Type converter type</param>
        void ConvertUsing(Type typeConverterType);

        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <param name="typeOverride"></param>
        void As(Type typeOverride);

        /// <summary>
        /// Assign a profile to the current type map
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Itself</returns>
        IMappingExpression WithProfile(string profileName);

        /// <summary>
        /// Customize individual members
        /// </summary>
        /// <param name="name">Name of the member</param>
        /// <param name="memberOptions">Callback for configuring member</param>
        /// <returns>Itself</returns>
        IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Customize configuration for an individual source member
        /// </summary>
        /// <param name="sourceMemberName">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);
    }

    /// <summary>
    /// Mapping configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public interface IMappingExpression<TSource, TDestination>
    {
        /// <summary>
        /// Customize configuration for individual member
        /// </summary>
        /// <param name="destinationMember">Expression to the top-level destination member. This must be a member on the <typeparamref name="TDestination"/>TDestination</param> type
        /// <param name="memberOptions">Callback for member options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember, Action<IMemberConfigurationExpression<TSource>> memberOptions);

        /// <summary>
        /// Customize configuration for individual member. Used when the name isn't known at compile-time
        /// </summary>
        /// <param name="name">Destination member name</param>
        /// <param name="memberOptions">Callback for member options</param>
        /// <returns></returns>
        IMappingExpression<TSource, TDestination> ForMember(string name, Action<IMemberConfigurationExpression<TSource>> memberOptions);

        /// <summary>
        /// Customize configuration for all members
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions);

        /// <summary>
        /// Include this configuration in derived types' maps
        /// </summary>
        /// <typeparam name="TOtherSource">Derived source type</typeparam>
        /// <typeparam name="TOtherDestination">Derived destination type</typeparam>
        /// <returns></returns>
        IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination;

        /// <summary>
        /// Assign a profile to the current type map
        /// </summary>
        /// <param name="profileName">Name of the profile</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> WithProfile(string profileName);

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <param name="mappingFunction">Callback to convert from source type to destination type</param>
        void ConvertUsing(Func<TSource, TDestination> mappingFunction);

        /// <summary>
        /// Skip member mapping and use a custom type converter instance to convert to the destination type
        /// </summary>
        /// <param name="converter">Type converter instance</param>
        void ConvertUsing(ITypeConverter<TSource, TDestination> converter);

        /// <summary>
        /// Skip member mapping and use a custom type converter instance to convert to the destination type
        /// </summary>
        /// <typeparam name="TTypeConverter">Type converter type</typeparam>
        void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction);

        /// <summary>
        /// Execute a custom mapping action before member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);

        /// <summary>
        /// Execute a custom mapping action after member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Supply a custom instantiation function for the destination type
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor);

        /// <summary>
        /// Supply a custom instantiation function for the destination type, based on the entire resolution context
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the current resolution context</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<ResolutionContext, TDestination> ctor);

        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <typeparam name="T">Destination type to use</typeparam>
        void As<T>();

        /// <summary>
        /// For self-referential types, limit recurse depth
        /// </summary>
        /// <param name="depth">Number of levels to limit to</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);

        /// <summary>
        /// Construct the destination object using the service locator
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructUsingServiceLocator();

        /// <summary>
        /// Create a type mapping from the destination to the source type, using the <typeparamref name="TDestination"/> members as validation
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TDestination, TSource> ReverseMap();

        /// <summary>
        /// Customize configuration for an individual source member
        /// </summary>
        /// <param name="sourceMember">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression<TSource>> memberOptions);

        /// <summary>
        /// Customize configuration for an individual source member. Member name not known until runtime
        /// </summary>
        /// <param name="sourceMemberName">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression<TSource>> memberOptions);
    }

    /// <summary>
    /// Configuration options for an individual member
    /// </summary>
    public interface IMemberConfigurationExpression
    {
        /// <summary>
        /// Map from a specific source member
        /// </summary>
        /// <param name="sourceMember">Source member to map from</param>
        void MapFrom(string sourceMember);

        /// <summary>
        /// Resolve destination member using a custom value resolver instance
        /// </summary>
        /// <param name="valueResolver">Value resolver to use</param>
        /// <returns>Value resolver configuration options</returns>
        IResolutionExpression ResolveUsing(IValueResolver valueResolver);

        /// <summary>
        /// Resolve destination member using a custom value resolver
        /// </summary>
        /// <param name="valueResolverType">Value resolver of type <see cref="IValueResolver"/></param>
        /// <returns>Value resolver configuration options</returns>
        IResolverConfigurationExpression ResolveUsing(Type valueResolverType);

        /// <summary>
        /// Resolve destination member using a custom value resolver
        /// </summary>
        /// <typeparam name="TValueResolver">Value resolver of type <see cref="IValueResolver"/></typeparam>
        /// <returns>Value resolver configuration options</returns>
        IResolverConfigurationExpression ResolveUsing<TValueResolver>();

        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
    }

    /// <summary>
    /// Source member configuration options
    /// </summary>
    public interface ISourceMemberConfigurationExpression
    {
        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
    }

    /// <summary>
    /// Source member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    public interface ISourceMemberConfigurationExpression<TSource> : ISourceMemberConfigurationExpression
    {
    }

    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    public interface IMemberConfigurationExpression<TSource>
    {
        [Obsolete("Formatters should not be used.")]
        void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression AddFormatter(Type valueFormatterType);
        [Obsolete("Formatters should not be used.")]
        void AddFormatter(IValueFormatter formatter);

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
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// This method can be used in mapping to LINQ query projections, while ResolveUsing cannot.
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);

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
}
