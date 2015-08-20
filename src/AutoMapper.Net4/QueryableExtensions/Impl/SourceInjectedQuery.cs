namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class SourceInjectedQuery<TSource, TDestination> : IOrderedQueryable<TDestination>
    {
        public SourceInjectedQuery(IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IMappingEngine mappingEngine, SourceInjectedQueryInspector inspector = null)
        {
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            Provider = new SourceInjectedQueryProvider<TSource, TDestination>(mappingEngine, dataSource, destQuery)
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

        public Type ElementType { get; }
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
    }


}
