using System;
using System.Collections.Generic;
using AutoMapper.Configuration;

namespace AutoMapper
{
    using StringDictionary = Dictionary<string, object>;

    public class MappingOperationOptions<TSource, TDestination> : IMappingOperationOptions<TSource, TDestination>
    {
        private StringDictionary _items;
        private static readonly Action<TSource, TDestination> Empty = (_, __) => { };

        public MappingOperationOptions(Func<Type, object> serviceCtor)
        {
            BeforeMapAction = AfterMapAction = Empty;
            ServiceCtor = serviceCtor;
        }

        public Func<Type, object> ServiceCtor { get; private set; }
        public IDictionary<string, object> Items => _items ?? (_items = new StringDictionary());
        public Action<TSource, TDestination> BeforeMapAction { get; protected set; }
        public Action<TSource, TDestination> AfterMapAction { get; protected set; }
        public ITypeMapConfiguration InlineConfiguration { get; protected set; } = new MappingExpression<TSource,TDestination>(MemberList.Destination);

        public void BeforeMap(Action<TSource, TDestination> beforeFunction) => BeforeMapAction = beforeFunction;

        public void AfterMap(Action<TSource, TDestination> afterFunction) => AfterMapAction = afterFunction;

        public IMappingExpression<TSource, TDestination> ConfigureMap()
            => ConfigureMap(MemberList.Destination);

        public IMappingExpression<TSource, TDestination> ConfigureMap(MemberList memberList)
        {
            var typeMapConfiguration = new MappingExpression<TSource, TDestination>(memberList);

            InlineConfiguration = typeMapConfiguration;

            return typeMapConfiguration;
        }

        public T CreateInstance<T>()
        {
            var service = ServiceCtor(typeof(T));
            if(service == null)
            {
                throw new AutoMapperMappingException("Cannot create an instance of type " + typeof(T));
            }
            return (T) service;
        }

        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            var ctor = ServiceCtor;
            ServiceCtor = t => constructor(t) ?? ctor(t);
        }

        void IMappingOperationOptions.BeforeMap(Action<object, object> beforeFunction) => BeforeMapAction = (s, d) => beforeFunction(s, d);

        void IMappingOperationOptions.AfterMap(Action<object, object> afterFunction) => AfterMapAction = (s, d) => afterFunction(s, d);
    }
}