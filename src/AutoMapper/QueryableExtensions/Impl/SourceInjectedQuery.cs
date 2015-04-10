using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class SourceInjectedQueryInspector
    {
        public SourceInjectedQueryInspector()
        {
            SourceResult = (e,o) => { };
            DestResult = o => { };
            StartQueryExecuteInterceptor = (t, e) => { };
        }
        public Action<Expression, object> SourceResult { get; set; }
        public Action<object> DestResult { get; set; }
        public Action<Type, Expression> StartQueryExecuteInterceptor { get; set; }

    }
    public class SourceInjectedQuery<TSource, TDestination> : IOrderedQueryable<TDestination>
    {
        public SourceInjectedQuery(IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IMappingEngine mappingEngine, SourceInjectedQueryInspector inspector = null)
        {
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            Provider = new SourceInjectedQueryProvider<TSource, TDestination>(this, mappingEngine, dataSource, destQuery)
            {
                Inspector = inspector ?? new SourceInjectedQueryInspector()
            };
        }

        internal SourceInjectedQuery(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
            ElementType = typeof(TDestination);
        }

        public IEnumerator<TDestination> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TDestination>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQueryProvider Provider { get; private set; }
    }


}
