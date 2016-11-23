namespace AutoMapper
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    /// Mapping configuration options for non-generic maps
    /// </summary>
    public interface IMappingExpression
    {
        /// <summary>
        /// Preserve object identity. Useful for circular references.
        /// </summary>
        /// <returns></returns>
        IMappingExpression PreserveReferences();

        /// <summary>
        /// Customize configuration for individual constructor parameter
        /// </summary>
        /// <param name="ctorParamName">Constructor parameter name</param>
        /// <param name="paramOptions">Options</param>
        /// <returns>Itself</returns>
        IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions);

        /// <summary>
        /// Create a type mapping from the destination to the source type, using the destination members as validation.
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression ReverseMap();

        /// <summary>
        /// Replace the original runtime instance with a new source instance. Useful when ORMs return proxy types with no relationships to runtime types.
        /// The returned source object will be mapped instead of what was supplied in the original source object.
        /// </summary>
        /// <param name="substituteFunc">Substitution function</param>
        /// <returns>New source object to map.</returns>
        IMappingExpression Substitute(Func<object, object> substituteFunc);
        
        /// <summary>
        /// Construct the destination object using the service locator
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression ConstructUsingServiceLocator();

        /// <summary>
        /// For self-referential types, limit recurse depth.
        /// Enables PreserveReferences.
        /// </summary>
        /// <param name="depth">Number of levels to limit to</param>
        /// <returns>Itself</returns>
        IMappingExpression MaxDepth(int depth);
        
        /// <summary>
        /// Supply a custom instantiation expression for the destination type for LINQ projection
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        IMappingExpression ConstructProjectionUsing(LambdaExpression ctor);

        /// <summary>
        /// Supply a custom instantiation function for the destination type, based on the entire resolution context
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object and current resolution context</param>
        /// <returns>Itself</returns>
        IMappingExpression ConstructUsing(Func<object, ResolutionContext, object> ctor);

        /// <summary>
        /// Supply a custom instantiation function for the destination type
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        IMappingExpression ConstructUsing(Func<object, object> ctor);

        /// <summary>
        /// Skip member mapping and use a custom expression during LINQ projection
        /// </summary>
        /// <param name="projectionExpression">Projection expression</param>
        void ProjectUsing(Expression<Func<object, object>> projectionExpression);

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
        /// Customize configuration for an individual source member
        /// </summary>
        /// <param name="sourceMemberName">Source member name</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);
        
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
        /// Customize individual members
        /// </summary>
        /// <param name="name">Name of the member</param>
        /// <param name="memberOptions">Callback for configuring member</param>
        /// <returns>Itself</returns>
        IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Include this configuration in derived types' maps
        /// </summary>
        /// <param name="derivedSourceType">Derived source type</param>
        /// <param name="derivedDestinationType">Derived destination type</param>
        /// <returns>Itself</returns>
        IMappingExpression Include(Type derivedSourceType, Type derivedDestinationType);

        /// <summary>
        /// Ignores all destination properties that have either a private or protected setter, forcing the mapper to respect encapsulation (note: order matters, so place this before explicit configuration of any properties with an inaccessible setter)
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter();

        /// <summary>
        /// When using ReverseMap, ignores all source properties that have either a private or protected setter, keeping the reverse mapping consistent with the forward mapping (note: destination properties with an inaccessible setter may still be mapped unless IgnoreAllPropertiesWithAnInaccessibleSetter is also used)
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        /// <summary>
        /// Include the base type map's configuration in this map
        /// </summary>
        /// <param name="sourceBase">Base source type</param>
        /// <param name="destinationBase">Base destination type</param>
        /// <returns></returns>
        IMappingExpression IncludeBase(Type sourceBase, Type destinationBase);

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression BeforeMap(Action<object, object> beforeFunction);

        /// <summary>
        /// Execute a custom mapping action before member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression BeforeMap<TMappingAction>()
            where TMappingAction : IMappingAction<object, object>;

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression AfterMap(Action<object, object> afterFunction);

        /// <summary>
        /// Execute a custom mapping action after member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression AfterMap<TMappingAction>()
            where TMappingAction : IMappingAction<object, object>;
    }

    /// <summary>
    /// Mapping configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public interface IMappingExpression<TSource, TDestination>
    {
        /// <summary>
        /// Preserve object identity. Useful for circular references.
        /// </summary>
        /// <returns></returns>
        IMappingExpression<TSource, TDestination> PreserveReferences();

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
        /// <returns></returns>
        IMappingExpression<TSource, TDestination> ForMember(string name,
            Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

        /// <summary>
        /// Customize configuration for all members
        /// </summary>
        /// <param name="memberOptions">Callback for member options</param>
        void ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

        /// <summary>
        /// Ignores all <typeparamref name="TDestination"/> properties that have either a private or protected setter, forcing the mapper to respect encapsulation (note: order matters, so place this before explicit configuration of any properties with an inaccessible setter)
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter();

        /// <summary>
        /// When using ReverseMap, ignores all <typeparamref name="TSource"/> properties that have either a private or protected setter, keeping the reverse mapping consistent with the forward mapping (note: <typeparamref name="TDestination"/> properties with an inaccessible setter may still be mapped unless IgnoreAllPropertiesWithAnInaccessibleSetter is also used)
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

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
        /// Include this configuration in derived types' maps
        /// </summary>
        /// <param name="derivedSourceType">Derived source type</param>
        /// <param name="derivedDestinationType">Derived destination type</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> Include(Type derivedSourceType, Type derivedDestinationType);

        /// <summary>
        /// Skip member mapping and use a custom expression during LINQ projection
        /// </summary>
        /// <param name="projectionExpression">Projection expression</param>
        void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression);

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <param name="mappingFunction">Callback to convert from source type to destination type</param>
        void ConvertUsing(Func<TSource, TDestination> mappingFunction);

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <param name="mappingFunction">Callback to convert from source type to destination type, including destination object</param>
        void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction);

        /// <summary>
        /// Skip member mapping and use a custom function to convert to the destination type
        /// </summary>
        /// <param name="mappingFunction">Callback to convert from source type to destination type, with source, destination and context</param>
        void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction);

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
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction);

        /// <summary>
        /// Execute a custom mapping action before member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>()
            where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction);

        /// <summary>
        /// Execute a custom mapping action after member mapping
        /// </summary>
        /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>()
            where TMappingAction : IMappingAction<TSource, TDestination>;

        /// <summary>
        /// Supply a custom instantiation function for the destination type
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor);

        /// <summary>
        /// Supply a custom instantiation expression for the destination type for LINQ projection
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the source object</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor);

        /// <summary>
        /// Supply a custom instantiation function for the destination type, based on the entire resolution context
        /// </summary>
        /// <param name="ctor">Callback to create the destination type given the current resolution context</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor);

        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <typeparam name="T">Destination type to use</typeparam>
        void As<T>() where T : TDestination;

        /// <summary>
        /// For self-referential types, limit recurse depth.
        /// Enables PreserveReferences.
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
        IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember,
            Action<ISourceMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Customize configuration for an individual source member. Member name not known until runtime
        /// </summary>
        /// <param name="sourceMemberName">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
        /// <param name="memberOptions">Callback for member configuration options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName,
            Action<ISourceMemberConfigurationExpression> memberOptions);

        /// <summary>
        /// Replace the original runtime instance with a new source instance. Useful when ORMs return proxy types with no relationships to runtime types.
        /// The returned source object will be mapped instead of what was supplied in the original source object.
        /// </summary>
        /// <param name="substituteFunc">Substitution function</param>
        /// <returns>New source object to map.</returns>
        IMappingExpression<TSource, TDestination> Substitute<TSubstitute>(Func<TSource, TSubstitute> substituteFunc);

        /// <summary>
        /// Customize configuration for individual constructor parameter
        /// </summary>
        /// <param name="ctorParamName">Constructor parameter name</param>
        /// <param name="paramOptions">Options</param>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions);

        /// <summary>
        /// Disable constructor validation. During mapping this map is used against an existing destination object and never constructed itself.
        /// </summary>
        /// <returns>Itself</returns>
        IMappingExpression<TSource, TDestination> DisableCtorValidation();
    }
}