using System;
using System.Collections.Generic;

namespace AutoMapper
{
    using StringDictionary = Dictionary<string, object>;

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
        private StringDictionary _items;
        private static readonly Action<object, object> Empty = (_, __) => { };

        public MappingOperationOptions(Func<Type, object> serviceCtor)
        {
            BeforeMapAction = AfterMapAction = Empty;
            ServiceCtor = serviceCtor;
            PreserveReferences = true;
        }

        public Func<Type, object> ServiceCtor { get; private set; }
        public IDictionary<string, object> Items => _items ?? (_items = new StringDictionary());
        public bool PreserveReferences { get; private set; }
        public Action<object, object> BeforeMapAction { get; protected set; }
        public Action<object, object> AfterMapAction { get; protected set; }

        public void DoNotPreserveReferences()
        {
            PreserveReferences = false;
        }

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
            var ctor = ServiceCtor;
            ServiceCtor = t => constructor(t) ?? ctor(t);
        }
    }
}