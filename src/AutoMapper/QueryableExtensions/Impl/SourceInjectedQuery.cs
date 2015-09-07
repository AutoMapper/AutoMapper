﻿using System.Security.Cryptography.X509Certificates;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class SourceSourceInjectedQuery<TSource, TDestination> : IOrderedQueryable<TDestination>, ISourceInjectedQueryable<TDestination>
    {
        private readonly Action<Exception> _exceptionHandler;

        public SourceSourceInjectedQuery(IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IMappingEngine mappingEngine, 
                IEnumerable<ExpressionVisitor> beforeVisitors,
                IEnumerable<ExpressionVisitor> afterVisitors,
                Action<Exception> exceptionHandler,
                SourceInjectedQueryInspector inspector = null)
        {
            _exceptionHandler = exceptionHandler ?? ((x)=> {});
            EnumerationHandler = (x => x);
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            Provider = new SourceInjectedQueryProvider<TSource, TDestination>(mappingEngine, dataSource, destQuery, beforeVisitors, afterVisitors, exceptionHandler)
            {
                Inspector = inspector ?? new SourceInjectedQueryInspector()
            };
        }

        internal SourceSourceInjectedQuery(IQueryProvider provider, Expression expression, Func<IEnumerator<object>, IEnumerator<object>> enumerationHandler, Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? ((x) => { });
            Provider = provider;
            Expression = expression;
            EnumerationHandler = enumerationHandler ?? (x => x);
            ElementType = typeof(TDestination);
        }

        public IQueryable<TDestination> OnEnumerated(Func<IEnumerator<object>, IEnumerator<object>> enumerationHandler)
        {
            EnumerationHandler = enumerationHandler ?? (x => x);
            ((SourceInjectedQueryProvider<TSource, TDestination>) Provider).EnumerationHandler = EnumerationHandler;
            return this;
        }

        internal Func<IEnumerator<object>, IEnumerator<object>> EnumerationHandler { get; set; }

        public IEnumerator<TDestination> GetEnumerator()
        {
            try
            {
                var enumerator = Provider.Execute<IEnumerable<TDestination>>(Expression).GetEnumerator();
                return (IEnumerator<TDestination>) EnumerationHandler((IEnumerator<object>) enumerator);
            }
            catch (Exception x)
            {
                _exceptionHandler(x);
                throw;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType { get; }
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
    }
}
