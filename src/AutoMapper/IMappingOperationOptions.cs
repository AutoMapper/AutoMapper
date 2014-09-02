using System;

namespace AutoMapper
{
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
        /// Create any missing type maps, if found
        /// </summary>
        bool CreateMissingTypeMaps { get; set; }

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
        /// Execute a custom function to the source and/or destination types before member mapping
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
        /// Execute a custom function to the source and/or destination types before member mapping
        /// </summary>
        /// <param name="afterFunction">Callback for the source/destination types</param>
        void AfterMap(Action<TSource, TDestination> afterFunction);
    }

    public class MappingOperationOptions : IMappingOperationOptions
    {
        public MappingOperationOptions()
        {
            Items = new Dictionary<string, object>();
            BeforeMapAction = (src, dest) => { };
            AfterMapAction = (src, dest) => { };
        }

        public Func<Type, object> ServiceCtor { get; private set; }
        public bool CreateMissingTypeMaps { get; set; }
        public IDictionary<string, object> Items { get; private set; }
        public bool DisableCache { get; set; }
        public Action<object, object> BeforeMapAction { get; protected set; }
        public Action<object, object> AfterMapAction { get; protected set; }

        public void BeforeMap(Action<object, object> beforeFunction)
        {
            BeforeMapAction = beforeFunction;
        }

        public void AfterMap(Action<object, object> afterFunction)
        {
            AfterMapAction = afterFunction;
        }

        void IMappingOperationOptions.ConstructServicesUsing(Func<Type, object> constructor)
        {
            ServiceCtor = constructor;
        }
    }

    public class MappingOperationOptions<TSource, TDestination> : MappingOperationOptions, IMappingOperationOptions<TSource, TDestination>
    {
        public void BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            BeforeMapAction = (src, dest) => beforeFunction((TSource) src, (TDestination) dest);
        }

        public void AfterMap(Action<TSource, TDestination> afterFunction)
        {
            AfterMapAction = (src, dest) => afterFunction((TSource)src, (TDestination)dest);
        }
    }
}
