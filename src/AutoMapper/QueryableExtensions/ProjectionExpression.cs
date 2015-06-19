namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class ProjectionExpression : IProjectionExpression
    {
        private static readonly MethodInfo QueryableSelectMethod = FindQueryableSelectMethod();

        private readonly IQueryable _source;
        private readonly IMappingEngine _mappingEngine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mappingEngine"></param>
        public ProjectionExpression(IQueryable source, IMappingEngine mappingEngine)
        {
            _source = source;
            _mappingEngine = mappingEngine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static MethodInfo FindQueryableSelectMethod()
        {
            //TODO: could it be this that is causing the issue?
            Expression<Func<IQueryable<object>>> select
                = () => default(IQueryable<object>).Select(default(Expression<Func<object, object>>));
            var method = ((MethodCallExpression) select.Body).Method.GetGenericMethodDefinition();
            return method;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(object parameters = null)
        {
            return To<TResult>(parameters, new string[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="membersToExpand"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand)
        {
            var paramValues = (parameters ?? new object()).GetType()
                .GetDeclaredProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));

            return To<TResult>(paramValues, membersToExpand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters)
        {
            return To<TResult>(parameters, new string[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="membersToExpand"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters,
            params string[] membersToExpand)
        {
            var expr = _mappingEngine.CreateMapExpression(_source.ElementType, typeof (TResult), parameters,
                membersToExpand);

            return _source.Provider.CreateQuery<TResult>(Expression.Call(null,
                QueryableSelectMethod.MakeGenericMethod(_source.ElementType, typeof (TResult)),
                new[] {_source.Expression, Expression.Quote(expr)}));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="membersToExpand"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(object parameters = null,
            params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = membersToExpand.Select(expr =>
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expr);
                return visitor.MemberName;
            }).ToArray();
            return To<TResult>(parameters, members);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="membersToExpand"></param>
        /// <returns></returns>
        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters,
            params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = membersToExpand.Select(expr =>
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expr);
                return visitor.MemberName;
            }).ToArray();

            var mapExpr = _mappingEngine.CreateMapExpression(_source.ElementType, typeof (TResult), parameters, members);

            return _source.Provider.CreateQuery<TResult>(Expression.Call(null,
                QueryableSelectMethod.MakeGenericMethod(_source.ElementType, typeof (TResult)),
                new[] {_source.Expression, Expression.Quote(mapExpr)}));
        }

        /// <summary>
        /// 
        /// </summary>
        private class MemberVisitor : ExpressionVisitor
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitMember(MemberExpression node)
            {
                MemberName = node.Member.Name;
                return base.VisitMember(node);
            }

            /// <summary>
            /// 
            /// </summary>
            public string MemberName { get; private set; }
        }
    }
}