using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl.QueryMapper
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

    public class SourceInjectedQuery<TSource, TDestination> : IQueryable<TDestination>, IOrderedQueryable<TDestination>
    {
        private readonly IQueryable<TSource> _dataSource;
        private SourceInjectedQueryProvider<TSource, TDestination> _provider;

        public SourceInjectedQuery(IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IMappingEngine mappingEngine, SourceInjectedQueryInspector inspector = null)
        {
            _dataSource = dataSource;
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            _provider = new SourceInjectedQueryProvider<TSource, TDestination>(this, mappingEngine, dataSource)
            {
                Inspector = inspector ?? new SourceInjectedQueryInspector()
            };
        }

        internal SourceInjectedQuery(SourceInjectedQueryProvider<TSource, TDestination> provider, Expression expression,
            IQueryable<TSource> dataSource)
        {
            _provider = provider;
            _dataSource = dataSource;
            Expression = expression;
            ElementType = typeof(TDestination);
        }

        public IEnumerator<TDestination> GetEnumerator()
        {
            System.Diagnostics.Debugger.Break();
            var sourceExpression = _provider.ConvertDestinationExpressionToSourceExpression(Expression);
            IQueryable<TSource> query = _dataSource.Provider.CreateQuery<TSource>(sourceExpression);
            return query.Project().To<TDestination>().GetEnumerator();

            //return Provider.Execute<IEnumerable<TDestination>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQueryProvider Provider { get { return _provider; } }
    }


}
