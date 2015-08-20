namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Internal;
    using Mappers;

    public class SourceInjectedQueryProvider<TSource, TDestination> : IQueryProvider
    {
        private readonly IMappingEngine _mappingEngine;
        private readonly IQueryable<TSource> _dataSource;
        private readonly IQueryable<TDestination> _destQuery;

        public SourceInjectedQueryProvider(IMappingEngine mappingEngine,
            IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery)
        {
            _mappingEngine = mappingEngine;
            _dataSource = dataSource;
            _destQuery = destQuery;
        }

        public SourceInjectedQueryInspector Inspector { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            return new SourceInjectedQuery<TSource, TDestination>(this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SourceInjectedQuery<TSource, TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            Inspector.StartQueryExecuteInterceptor(null, expression);

            var sourceExpression = ConvertDestinationExpressionToSourceExpression(expression);
            var sourceResult = InvokeSourceQuery(null, sourceExpression);

            Inspector.SourceResult(sourceExpression, sourceResult);
            return sourceResult;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var resultType = typeof (TResult);
            Inspector.StartQueryExecuteInterceptor(resultType, expression);

            var sourceExpression = ConvertDestinationExpressionToSourceExpression(expression);

            var destResultType = typeof(TResult);
            var sourceResultType = CreateSourceResultType(destResultType);

            var sourceResult = InvokeSourceQuery(sourceResultType, sourceExpression);

            Inspector.SourceResult(sourceExpression, sourceResult);

            object destResult;
            if (IsProjection<TDestination>(resultType))
                destResult = new ProjectionExpression(sourceResult as IQueryable<TSource>, _mappingEngine).To<TDestination>();
            else
                destResult = _mappingEngine.Map(sourceResult, sourceResultType, destResultType);
            Inspector.DestResult(sourceResult);

            return (TResult)destResult;
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
            var typeMap = _mappingEngine.ConfigurationProvider.FindTypeMapFor(typeof (TDestination), typeof (TSource));
            var visitor = new ExpressionMapper.MappingVisitor(typeMap, _destQuery.Expression, _dataSource.Expression, null,
                new[] {typeof (TSource)});
            var sourceExpression = visitor.Visit(expression);
            return sourceExpression;
        }
    }
}