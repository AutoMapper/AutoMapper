﻿using System;
using System.Collections.Generic;

namespace AutoMapper
{
    /// <summary>
    /// Options for a single map operation
    /// </summary>
    public interface IMappingOperationOptions
    {
        T CreateInstance<T>();

        Func<Type, object> ServiceCtor { get; }

        /// <summary>
        /// Construct services using this callback. Use this for child/nested containers
        /// </summary>
        /// <param name="constructor"></param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        /// <summary>
        /// Add context items to be accessed at map time inside an <see cref="IValueResolver{TSource, TDestination, TMember}"/> or <see cref="ITypeConverter{TSource, TDestination}"/>
        /// </summary>
        IDictionary<string, object> Items { get; }

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

    public interface IMappingOperationOptions<out TSource, out TDestination> : IMappingOperationOptions
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