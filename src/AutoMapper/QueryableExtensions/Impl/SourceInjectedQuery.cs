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
                IMapper mapper, 
                IEnumerable<ExpressionVisitor> beforeVisitors,
                IEnumerable<ExpressionVisitor> afterVisitors,
                Action<Exception> exceptionHandler,
                SourceInjectedQueryInspector inspector = null)
        {
            _exceptionHandler = exceptionHandler ?? ((x)=> {});
            EnumerationHandler = (x => {});
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            Provider = new SourceInjectedQueryProvider<TSource, TDestination>(mapper, dataSource, destQuery, beforeVisitors, afterVisitors, exceptionHandler)
            {
                Inspector = inspector ?? new SourceInjectedQueryInspector()
            };
        }

        internal SourceSourceInjectedQuery(IQueryProvider provider, Expression expression, Action<IEnumerable<object>> enumerationHandler, Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? ((x) => {});
            Provider = provider;
            Expression = expression;
            EnumerationHandler = enumerationHandler ?? (x => {});
            ElementType = typeof(TDestination);
        }

        public IQueryable<TDestination> OnEnumerated(Action<IEnumerable<object>> enumerationHandler)
        {
            EnumerationHandler = enumerationHandler ?? (x => {});
            ((SourceInjectedQueryProvider<TSource, TDestination>) Provider).EnumerationHandler = EnumerationHandler;
            return this;
        }

        internal Action<IEnumerable<object>> EnumerationHandler { get; set; }

        public IEnumerator<TDestination> GetEnumerator()
        {
            try
            {
                var results = Provider.Execute<IEnumerable<TDestination>>(Expression).Cast<object>().ToArray();
                EnumerationHandler(results);
                return results.Cast<TDestination>().GetEnumerator();
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
