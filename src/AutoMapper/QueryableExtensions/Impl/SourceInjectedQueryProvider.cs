

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;
    using Mappers;
    using System.Collections.Generic;

    public class SourceInjectedQueryProvider<TSource, TDestination> : IQueryProvider
    {
        private readonly IMapper _mapper;
        private readonly IQueryable<TSource> _dataSource;
        private readonly IQueryable<TDestination> _destQuery;
        private readonly IEnumerable<ExpressionVisitor> _beforeVisitors;
        private readonly IEnumerable<ExpressionVisitor> _afterVisitors;
        private readonly System.Collections.Generic.IDictionary<string, object> _parameters;
        private readonly MemberInfo[] _membersToExpand;
        private readonly Action<Exception> _exceptionHandler;

        public SourceInjectedQueryProvider(IMapper mapper,
            IQueryable<TSource> dataSource, IQueryable<TDestination> destQuery,
                IEnumerable<ExpressionVisitor> beforeVisitors,
                IEnumerable<ExpressionVisitor> afterVisitors,
                Action<Exception> exceptionHandler, 
                System.Collections.Generic.IDictionary<string, object> parameters,
                MemberInfo[] membersToExpand)
        {
            _mapper = mapper;
            _dataSource = dataSource;
            _destQuery = destQuery;
            _beforeVisitors = beforeVisitors;
            _afterVisitors = afterVisitors;
            _parameters = parameters;
            _membersToExpand = membersToExpand ?? new MemberInfo[0];
            _exceptionHandler = exceptionHandler ?? ((x) => { }); ;
        }
        
        public SourceInjectedQueryInspector Inspector { get; set; }
        internal Action<IEnumerable<object>> EnumerationHandler { get; set; }

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


// TODO: Check out which is right.
/*                var destResult = IsProjection<TDestination>(resultType) 
                    ? new ProjectionExpression(sourceResult as IQueryable<TSource>, _mapper.ConfigurationProvider.ExpressionBuilder).To<TDestination>() 
                    : _mapper.Map(sourceResult, sourceResultType, destResultType);
*/
                object destResult;
                if (IsProjection<TDestination>(resultType))
                {
                    // in case of a projection, we need an IQueryable
                    var sourceResult = _dataSource.Provider.CreateQuery(sourceExpression);
                    Inspector.SourceResult(sourceExpression, sourceResult);

                    destResult =
                        new ProjectionExpression((IQueryable<TSource>) sourceResult, _mappingEngine).To<TDestination>(
                            _parameters, _membersToExpand);
                }
                else
                {
                    /* 
                        in case of an element result (so instead of IQueryable<TResult>, just TResult)
                        we still want to support parameters.
                        This is e.g. the case, when the developer writes "UseAsDataSource().For<TResult>().FirstOrDefault(x => ...)
                        To still be able to support parameters, we need to create a query from it. 
                        That said, we need to replace the "element" operator "FirstOrDefault" with a "Where" operator, then apply our "Select" 
                        to map from TSource to TResult and finally re-apply the "element" operator ("FirstOrDefault" in our case) so only
                        one element is returned by our "Execute<TResult>" method. Otherwise we'd get an InvalidCastException

                        * So first we visit the sourceExpression and replace "element operators" with "where"
                        * then we create our mapping expression from TSource to TDestination (select) and apply it
                        * finally, we search for the element expression overload of our replaced "element operator" that has no expression as parameter
                            this expression is not needed anymore as it has already been applied to the "Where" operation and can be safely omitted
                        * when we're done creating our new expression, we call the underlying provider to execute it
                    */

                    var rmv = new RetainQueryableVisitor<TSource>();

                    // default back to simple mapping of object instance for backwards compatibility (e.g. mapping non-nullable to nullable fields)
                    if (_parameters != null || _membersToExpand.Length != 0)
                    {
                        sourceExpression = rmv.Visit(sourceExpression);
                    }

                    if (rmv.FoundElementOperator)
                    {
                        /*  in case of primitive element operators (e.g. Any(), Sum()...)
                            we need to map the originating types (e.g. Entity to Dto) in this query
                            as the final value will be projected automatically
                            
                            == example 1 ==
                            UseAsDataSource().For<Dto>().Any(entity => entity.Name == "thename")
                            ..in that case sourceResultType and destResultType would both be "Boolean" which is not mappable for AutoMapper

                            == example 2 ==
                            UseAsDataSource().For<Dto>().FirstOrDefault(entity => entity.Name == "thename")
                            ..in this case the sourceResultType would be Entity and the destResultType Dto, so we can map the query directly
                        */

                        if (sourceResultType == destResultType)// && destResultType.IsPrimitive)
                        {
                            sourceResultType = typeof (TSource);
                            destResultType = typeof (TDestination);
                        }

                        var mapExpr = _mappingEngine.CreateMapExpression(sourceResultType, destResultType,
                            _parameters, _membersToExpand);
                        // add projection via "select" operator
                        var expr = Expression.Call(
                                null,
                                QueryableSelectMethod.MakeGenericMethod(sourceResultType, destResultType),
                                new[] {sourceExpression, Expression.Quote(mapExpr)}
                            );

                        // in case an element operator without predicate expression was found (and thus not replaced)
                        MethodInfo replacementMethod = rmv.ElementOperator;
                        // in case an element operator with predicate expression was replaced
                        if (rmv.ReplacedMethod != null) { 
                            // find replacement method that has no more predicates
                            replacementMethod = typeof (Queryable).GetMethods()
                                .Single(m => m.Name == rmv.ReplacedMethod.Name
                                            &&
                                            m.GetParameters()
                                                .All(p => typeof (Queryable).IsAssignableFrom(p.Member.ReflectedType))
                                            && m.GetParameters().Length == rmv.ReplacedMethod.GetParameters().Length - 1);
                        }

                        // reintroduce element operator that was replaced with a "where" operator to make it queryable
                        expr = Expression.Call(null,
                            replacementMethod.MakeGenericMethod(destResultType), expr);

                        destResult = _dataSource.Provider.Execute(expr);
                    }
                    // If there was no element operator that needed to be replaced by "where", just map the result
                    else
                    {
                        var sourceResult = _dataSource.Provider.Execute(sourceExpression);
                        Inspector.SourceResult(sourceExpression, sourceResult);
                        destResult = _mappingEngine.Map(sourceResult, sourceResultType, destResultType);
                    }
                }

                Inspector.DestResult(destResult);

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

        private static readonly MethodInfo QueryableSelectMethod = FindQueryableSelectMethod();

        private static MethodInfo FindQueryableSelectMethod()
        {
            Expression<Func<IQueryable<object>>> select = () => Queryable.Select(default(IQueryable<object>), default(Expression<Func<object, object>>));
            MethodInfo method = ((MethodCallExpression)select.Body).Method.GetGenericMethodDefinition();
            return method;
        }
    }

    internal class RetainQueryableVisitor<TDestination> : ExpressionVisitor
    {
        private static readonly MethodInfo QueryableWhereMethod = FindQueryableWhereMethod();
        private static readonly string[] IgnoredMethods = new[] {"Where", "Select", "OrderBy", "OrderByDescending"};
        private bool _ignoredMethodFound = false;

        private static MethodInfo FindQueryableWhereMethod()
        {
            Expression<Func<IQueryable<object>>> select = () => Queryable.Where(default(IQueryable<object>), default(Expression<Func<object, bool>>));
            MethodInfo method = ((MethodCallExpression)select.Body).Method.GetGenericMethodDefinition();
            return method;
        }
        
        public MethodInfo ReplacedMethod { get; private set; }

        public MethodInfo ElementOperator { get; private set; }

        public bool FoundElementOperator { get; private set; }
        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // only replace first occurrence
            if (ReplacedMethod != null || _ignoredMethodFound)
            {
                return base.VisitMethodCall(node);
            }

            if (node.Method.DeclaringType == typeof (System.Linq.Queryable)
                && !IgnoredMethods.Contains(node.Method.Name)
                && !typeof (IQueryable).IsAssignableFrom(node.Method.ReturnType))
            {
                var parameters = node.Method.GetParameters();

                if (parameters.Length > 1 &&
                    typeof (System.Linq.Expressions.Expression).IsAssignableFrom(parameters[1].ParameterType))
                {
                    FoundElementOperator = true;
                    ReplacedMethod = node.Method.GetGenericMethodDefinition();
                    return Expression.Call(null, QueryableWhereMethod.MakeGenericMethod(typeof (TDestination)),
                        node.Arguments);
                }
                // no predicate
                else if (parameters.Length == 1)
                {
                    FoundElementOperator = true;
                    ElementOperator = node.Method.GetGenericMethodDefinition();
                    return node.Arguments[0];
                }
            }
            else if(node.Method.DeclaringType == typeof(System.Linq.Queryable)
                && typeof(IQueryable).IsAssignableFrom(node.Method.ReturnType)
                && IgnoredMethods.Contains(node.Method.Name))
            {
                _ignoredMethodFound = true;
            }

            return base.VisitMethodCall(node);
        }
    }
}