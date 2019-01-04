using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper
{
    public interface IMappingExpressionBase<TSource, TDestination, out TMappingExpression> 
        where TMappingExpression : IMappingExpressionBase<TSource, TDestination, TMappingExpression>
    {
        /// <summary>
        /// Construct the destination object using the service locator
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression ConstructUsingServiceLocator();

        /// <summary>
        /// For self-referential types, limit recurse depth.
        /// Enables PreserveReferences.
        /// </summary>
        /// <param name="depth">Number of levels to limit to</param>
        /// <returns>Itself</returns>
        TMappingExpression MaxDepth(int depth);

        /// <summary>
        /// Preserve object identity. Useful for circular references.
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression PreserveReferences();

        /// <summary>
        /// Disable constructor validation. During mapping this map is used against an existing destination object and never constructed itself.
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression DisableCtorValidation();

        IList<ValueTransformerConfiguration> ValueTransformers { get; }

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        TMappingExpression BeforeMap(Action<TSource, TDestination> beforeFunction);

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        TMappingExpression BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction);

        /// <summary>
        /// Execute a custom mapping action before member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        TMappingExpression BeforeMap<TMappingAction>()
            where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        TMappingExpression AfterMap(Action<TSource, TDestination> afterFunction);

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        TMappingExpression AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction);

        /// <summary>
        /// Execute a custom mapping action after member mapping
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        TMappingExpression AfterMap<TMappingAction>()
            where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Specify which member list to validate
        /// </summary>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Itself</returns>
        TMappingExpression ValidateMemberList(MemberList memberList);

        /// <summary>
        /// Include this configuration in all derived types' maps. Works by scanning all type maps for matches during configuration.
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression IncludeAllDerived();

        /// <summary>
        /// Include this configuration in derived types' maps
        /// </summary>
        /// <param name="derivedSourceType">Derived source type</param>
        /// <param name="derivedDestinationType">Derived destination type</param>
        /// <returns>Itself</returns>
        TMappingExpression Include(Type derivedSourceType, Type derivedDestinationType);

        /// <summary>
        /// Include the base type map's configuration in this map
        /// </summary>
        /// <param name="sourceBase">Base source type</param>
        /// <param name="destinationBase">Base destination type</param>
        /// <returns></returns>
        TMappingExpression IncludeBase(Type sourceBase, Type destinationBase);

        /// <summary>
        /// Customize configuration for an individual source member. Member name not known until runtime
        /// </summary>
        /// <param name="sourceMemberName">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        TMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Ignores all <typeparamref name="TDestination"/> properties that have either a private or protected setter, forcing the mapper to respect encapsulation (note: order matters, so place this before explicit configuration of any properties with an inaccessible setter)
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter();

        /// <summary>
        /// When using ReverseMap, ignores all <typeparamref name="TSource"/> properties that have either a private or protected setter, keeping the reverse mapping consistent with the forward mapping (note: <typeparamref name="TDestination"/> properties with an inaccessible setter may still be mapped unless IgnoreAllPropertiesWithAnInaccessibleSetter is also used)
        /// </summary>
        /// <returns>Itself</returns>
        TMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        /// <summary>
        /// Supply a custom instantiation expression for the destination type
        /// </summary>
        /// <param name="ctor">Expression to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        TMappingExpression ConstructUsing(Expression<Func<TSource, TDestination>> ctor);

        /// <summary>
        /// Supply a custom instantiation function for the destination type, based on the entire resolution context
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="ctor">Callback to create the destination type given the current resolution context</param>
        /// <returns>Itself</returns>
        TMappingExpression ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor);

        /// <summary>
        /// Customize configuration for individual constructor parameter
        /// </summary>
        /// <param name="ctorParamName">Constructor parameter name</param>
        /// <param name="paramOptions">Options</param>
        /// <returns>Itself</returns>
        TMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions);

        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <param name="typeOverride"></param>
        void As(Type typeOverride);

        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// Use this method if you need to specify the converter type at runtime
        /// </summary>
        /// <param name="typeConverterType">Type converter type</param>
        void ConvertUsing(Type typeConverterType);

        /// <summary>
        /// Skip member mapping and use a custom expression to convert to the destination type
        /// </summary>
        /// <param name="mappingExpression">Callback to convert from source type to destination type</param>
        void ConvertUsing(Expression<Func<TSource, TDestination>> mappingExpression);
    }

    /// <summary>
    /// Mapping configuration options for non-generic maps
    /// </summary>
    public interface IMappingExpression : IMappingExpressionBase<object, object, IMappingExpression>
    {
        /// <summary>
        /// Create a type mapping from the destination to the source type, using the destination members as validation.
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression ReverseMap();

        /// <summary>
        /// Customize configuration for all members
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Customize configuration for members not previously configured
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllOtherMembers(Action<IMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// </summary>
        /// <typeparam name="TTypeConverter">Type converter type</typeparam>
        void ConvertUsing<TTypeConverter>();

        /// <summary>
        /// Customize individual members
        /// </summary>
        /// <param name="name">Name of the member</param>
        /// <param name="memberOptions">Callback for configuring member</param>
        /// <returns>Itself</returns>
        IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions);
    }

    /// <summary>
    /// Mapping configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public interface IMappingExpression<TSource, TDestination> : IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>
    {
        /// <summary>
        /// Customize configuration for a path inside the destination object.
        /// </summary>
        /// <param name="destinationMember">Expression to the destination sub object</param>
        /// <param name="memberOptions">Callback for member options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

        /// <summary>
        /// Customize configuration for members not previously configured
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllOtherMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

        /// <summary>
        /// Customize configuration for individual member
        /// </summary>
        /// <param name="destinationMember">Expression to the top-level destination member. This must be a member on the <typeparamref name="TDestination"/>TDestination</param> type
        /// <param name="memberOptions">Callback for member options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

        /// <summary>
        /// Customize configuration for individual member. Used when the name isn't known at compile-time
        /// </summary>
        /// <param name="name">Destination member name</param>
        /// <param name="memberOptions">Callback for member options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForMember(string name,
            Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

        /// <summary>
        /// Customize configuration for all members
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

        /// <summary>
        /// Include this configuration in derived types' maps
        /// </summary>
        /// <typeparam name="TOtherSource">Derived source type</typeparam>
        /// <typeparam name="TOtherDestination">Derived destination type</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination;

        /// <summary>
        /// Include the base type map's configuration in this map
        /// </summary>
        /// <typeparam name="TSourceBase">Base source type</typeparam>
        /// <typeparam name="TDestinationBase">Base destination type</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>();

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="mappingFunction">Callback to convert from source type to destination type, including destination object</param>
        void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction);

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="mappingFunction">Callback to convert from source type to destination type, with source, destination and context</param>
        void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction);

        /// <summary>
        /// Skip member mapping and use a custom type converter instance to convert to the destination type
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="converter">Type converter instance</param>
        void ConvertUsing(ITypeConverter<TSource, TDestination> converter);

        /// <summary>
        /// Skip member mapping and use a custom type converter instance to convert to the destination type
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <typeparam name="TTypeConverter">Type converter type</typeparam>
        void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;


        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <typeparam name="T">Destination type to use</typeparam>
        void As<T>() where T : TDestination;

        /// <summary>
        /// Customize configuration for an individual source member
        /// </summary>
        /// <param name="sourceMember">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember,
            Action<ISourceMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <param name="transformer">Transformation expression</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer);

        /// <summary>
        /// Create a type mapping from the destination to the source type, using the <typeparamref name="TDestination"/> members as validation
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TDestination, TSource> ReverseMap();
    }
}