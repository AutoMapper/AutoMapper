using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using IObjectDictionary = System.Collections.Generic.IDictionary<string, object>;
    using MemberPaths = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<MemberInfo>>;

    public class SourceSourceInjectedQuery<TSource, TDestination> : IOrderedQueryable<TDestination>, ISourceInjectedQueryable<TDestination>
    {
        private readonly Action<Exception> _exceptionHandler;

        public SourceSourceInjectedQuery(IQueryable<TSource> dataSource,
                IQueryable<TDestination> destQuery,
                IMapper mapper,
                IEnumerable<ExpressionVisitor> beforeVisitors,
                IEnumerable<ExpressionVisitor> afterVisitors,
                Action<Exception> exceptionHandler,
                IObjectDictionary parameters,
                MemberPaths membersToExpand,
                SourceInjectedQueryInspector inspector)
        {
            Parameters = parameters;
            EnumerationHandler = (x => { });
            Expression = destQuery.Expression;
            ElementType = typeof(TDestination);
            Provider = new SourceInjectedQueryProvider<TSource, TDestination>(mapper, dataSource, destQuery, beforeVisitors, afterVisitors, exceptionHandler, parameters, membersToExpand)
            {
                Inspector = inspector ?? new SourceInjectedQueryInspector(),
            };
            _exceptionHandler = exceptionHandler ?? ((x) => { });
        }

        internal SourceSourceInjectedQuery(IQueryProvider provider, Expression expression, Action<IEnumerable<object>> enumerationHandler, Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? ((x) => { });
            Provider = provider;
            Expression = expression;
            EnumerationHandler = enumerationHandler ?? (x => { });
            ElementType = typeof(TDestination);
        }

        public IQueryable<TDestination> OnEnumerated(Action<IEnumerable<object>> enumerationHandler)
        {
            EnumerationHandler = enumerationHandler ?? (x => { });
            ((SourceInjectedQueryProvider<TSource, TDestination>)Provider).EnumerationHandler = EnumerationHandler;
            return this;
        }

        public IQueryable<TDestination> AsQueryable()
        {
            return this;
        }

        internal Action<IEnumerable<object>> EnumerationHandler { get; set; }
        internal IObjectDictionary Parameters { get; set; }

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
