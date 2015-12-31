namespace AutoMapper
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Options for a single map operation
    /// </summary>
    public interface IMappingOperationOptions
    {
        /// <summary>
        /// Construct services using this callback. Use this for child/nested containers
        /// </summary>
        /// <param name="constructor"></param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        /// <summary>
        /// Add context items to be accessed at map time inside an <see cref="IValueResolver"/> or <see cref="ITypeConverter{TSource, TDestination}"/>
        /// </summary>
        IDictionary<string, object> Items { get; }

        /// <summary>
        /// Disable the cache used to re-use destination instances based on equality
        /// </summary>
        bool DisableCache { get; set; }

        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        void BeforeMap(Action<object, object> beforeFunction);

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        void AfterMap(Action<object, object> afterFunction);
    }

    public interface IMappingOperationOptions<TSource, TDestination> : IMappingOperationOptions
    {
        /// <summary>
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="beforeFunction">Callback for the source/destination types</param>
        void BeforeMap(Action<TSource, TDestination> beforeFunction);

        /// <summary>
        /// Execute a custom function to the source and/or destination types after member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        void AfterMap(Action<TSource, TDestination> afterFunction);
    }
}