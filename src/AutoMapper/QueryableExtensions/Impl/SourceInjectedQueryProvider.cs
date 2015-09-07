using System.Collections.Generic;
using System.ComponentModel;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Internal;
    using Mappers;

    public class SourceInjectedQueryProvider<TSource, TDestination> : IQueryProvider
    {
        private readonly IMapper _mapper;
        private readonly IQueryable<TSource> _dataSource;
        private readonly IQueryable<TDestination> _destQuery;
        private readonly IEnumerable<ExpressionVisitor> _beforeVisitors;
        private readonly IEnumerable<ExpressionVisitor> _afterVisitors;
        private readonly Action<Exception> _exceptionHandler;

        public SourceInjectedQueryProvider(IMapper mapper,
            IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IEnumerable<ExpressionVisitor> beforeVisitors,
                IEnumerable<ExpressionVisitor> afterVisitors,
                Action<Exception> exceptionHandler)
        {
            _mapper = mapper;
            _dataSource = dataSource;
            _destQuery = destQuery;
            _beforeVisitors = beforeVisitors;
            _afterVisitors = afterVisitors;
            _exceptionHandler = exceptionHandler ?? ((x) => { }); ;
        }

        public SourceInjectedQueryInspector Inspector { get; set; }
        internal Func<IEnumerator<object>, IEnumerator<object>> EnumerationHandler { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            return new SourceSourceInjectedQuery<TSource, TDestination>(this, expression, EnumerationHandler, _exceptionHandler);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SourceSourceInjectedQuery<TSource, TElement>(this, expression, EnumerationHandler, _exceptionHandler);
        }

        public object Execute(Expression expression)
        {
            try
            {
                Inspector.StartQueryExecuteInterceptor(null, expression);

                var sourceExpression = ConvertDestinationExpressionToSourceExpression(expression);
                var sourceResult = InvokeSourceQuery(null, sourceExpression);

                Inspector.SourceResult(sourceExpression, sourceResult);
                return sourceResult;
            }
            catch (Exception x)
            {
                _exceptionHandler(x);
                throw;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            try
            {
                var resultType = typeof (TResult);
                Inspector.StartQueryExecuteInterceptor(resultType, expression);

                var sourceExpression = ConvertDestinationExpressionToSourceExpression(expression);

                var destResultType = typeof (TResult);
                var sourceResultType = CreateSourceResultType(destResultType);

                var sourceResult = InvokeSourceQuery(sourceResultType, sourceExpression);

                Inspector.SourceResult(sourceExpression, sourceResult);

                var destResult = IsProjection<TDestination>(resultType) 
                    ? new ProjectionExpression(sourceResult as IQueryable<TSource>, _mapper.ConfigurationProvider.ExpressionBuilder).To<TDestination>() 
                    : _mapper.Map(sourceResult, sourceResultType, destResultType);
                Inspector.DestResult(sourceResult);

                // implicitly convert types in case of valuetypes which cannot be casted explicitly
                if (typeof (TResult).IsValueType && destResult.GetType() != typeof (TResult))
                    return (TResult) Convert.ChangeType(destResult, typeof (TResult));

                // if it is not a valuetype, we can safely cast it
                return (TResult) destResult;
            }
            catch (Exception x)
            {
                _exceptionHandler(x);
                throw;
            }
        }

        private object InvokeSourceQuery(Type sourceResultType, Expression sourceExpression)
        {
            var result = IsProjection<TSource>(sourceResultType)
                ? _dataSource.Provider.CreateQuery(sourceExpression)
                : _dataSource.Provider.Execute(sourceExpression);
            return result;
        }

        private static bool IsProjection<T>(Type resultType)
        {
            return resultType.IsEnumerableType() && !resultType.IsQueryableType() && resultType != typeof(string) && resultType.GetGenericElementType() == typeof(T);
        }

        private static Type CreateSourceResultType(Type destResultType)
        {
            var sourceResultType = destResultType.ReplaceItemType(typeof(TDestination), typeof(TSource));
            return sourceResultType;
        }

        private Expression ConvertDestinationExpressionToSourceExpression(Expression expression)
        {
            // call beforevisitors
            expression = _beforeVisitors.Aggregate(expression, (current, before) => before.Visit(current));

            var typeMap = _mappingEngine.ConfigurationProvider.FindTypeMapFor(typeof (TDestination), typeof (TSource));
            var visitor = new ExpressionMapper.MappingVisitor(typeMap, _destQuery.Expression, _dataSource.Expression, null,
                new[] {typeof (TSource)});
            var sourceExpression = visitor.Visit(expression);

            // call aftervisitors
            sourceExpression = _afterVisitors.Aggregate(sourceExpression, (current, after) => after.Visit(current));

            return sourceExpression;
        }
    }
}