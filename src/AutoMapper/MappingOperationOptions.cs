namespace AutoMapper
{
    using System;
    using System.Collections.Generic;

    public class MappingOperationOptions<TSource, TDestination> : MappingOperationOptions,
        IMappingOperationOptions<TSource, TDestination>
    {
        public MappingOperationOptions(Func<Type, object> serviceCtor) : base(serviceCtor)
        {
        }

        public void BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            BeforeMapAction = (src, dest) => beforeFunction((TSource) src, (TDestination) dest);
        }

        public void AfterMap(Action<TSource, TDestination> afterFunction)
        {
            AfterMapAction = (src, dest) => afterFunction((TSource) src, (TDestination) dest);
        }
    }

    public class MappingOperationOptions : IMappingOperationOptions
    {
        public MappingOperationOptions(Func<Type, object> serviceCtor)
        {
            Items = new Dictionary<string, object>();
            BeforeMapAction = (src, dest) => { };
            AfterMapAction = (src, dest) => { };
            ServiceCtor = serviceCtor;
        }

        public Func<Type, object> ServiceCtor { get; private set; }
        public IDictionary<string, object> Items { get; }
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
}