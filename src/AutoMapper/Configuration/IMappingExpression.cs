namespace AutoMapper;
/// <summary>
/// Mapping configuration options for non-generic maps
/// </summary>
public interface IMappingExpression : IMappingExpressionBase<object, object, IMappingExpression>
{
    /// <summary>
    /// Add extra configuration to the current map by also mapping the specified child objects to the destination object.
    /// The maps from the child types to the destination need to be created explicitly.
    /// </summary>
    /// <param name="memberNames">the names of child object properties to map to the destination</param>
    /// <returns></returns>
    IMappingExpression IncludeMembers(params string[] memberNames);
    /// <summary>
    /// Create a type mapping from the destination to the source type, with validation disabled.
    /// This allows for two-way mapping.
    /// </summary>
    /// <returns>Itself</returns>
    IMappingExpression ReverseMap();
    /// <summary>
    /// Customize configuration for all members
    /// </summary>
    /// <param name="memberOptions">Callback for member options</param>
    void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions);
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
public interface IMappingExpression<TSource, TDestination> : IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>,
    IProjectionExpression<TSource, TDestination, IMappingExpression<TSource, TDestination>>
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
    IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>() where TOtherSource : TSource where TOtherDestination : TDestination;
    /// <summary>
    /// Include the base type map's configuration in this map
    /// </summary>
    /// <typeparam name="TSourceBase">Base source type</typeparam>
    /// <typeparam name="TDestinationBase">Base destination type</typeparam>
    /// <returns>Itself</returns>
    IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>();
    /// <summary>
    /// Customize configuration for an individual source member
    /// </summary>
    /// <param name="sourceMember">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
    /// <param name="memberOptions">Callback for member configuration options</param>
    /// <returns>Itself</returns>
    IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember,
        Action<ISourceMemberConfigurationExpression> memberOptions);
    /// <summary>
    /// Create a type mapping from the destination to the source type, with validation disabled.
    /// This allows for two-way mapping.
    /// </summary>
    /// <returns>Itself</returns>
    IMappingExpression<TDestination, TSource> ReverseMap();
    /// <summary>
    /// Override the destination type mapping for looking up configuration and instantiation
    /// </summary>
    /// <typeparam name="T">Destination type to use</typeparam>
    void As<T>() where T : TDestination;
}
public interface IProjectionExpression<TSource, TDestination, TMappingExpression> : IProjectionExpressionBase<TSource, TDestination, TMappingExpression>
    where TMappingExpression : IProjectionExpressionBase<TSource, TDestination, TMappingExpression>
{
    /// <summary>
    /// Apply a transformation function after any resolved destination member value with the given type
    /// </summary>
    /// <typeparam name="TValue">Value type to match and transform</typeparam>
    /// <param name="transformer">Transformation expression</param>
    /// <returns>Itself</returns>
    TMappingExpression AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer);
    /// <summary>
    /// Add extra configuration to the current map by also mapping the specified child objects to the destination object.
    /// The maps from the child types to the destination need to be created explicitly.
    /// </summary>
    /// <param name="memberExpressions">the child objects to map to the destination</param>
    /// <returns></returns>
    TMappingExpression IncludeMembers(params Expression<Func<TSource, object>>[] memberExpressions);
}
public interface IProjectionExpression<TSource, TDestination> : IProjectionExpression<TSource, TDestination, IProjectionExpression<TSource, TDestination>>
{
    /// <summary>
    /// Customize configuration for individual member
    /// </summary>
    /// <param name="destinationMember">Expression to the top-level destination member. This must be a member on the <typeparamref name="TDestination"/>TDestination</param> type
    /// <param name="memberOptions">Callback for member options</param>
    /// <returns>Itself</returns>
    IProjectionExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
        Action<IProjectionMemberConfiguration<TSource, TDestination, TMember>> memberOptions);
}