namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Internal;
    using System.Reflection;

    public class ProjectionExpression : IProjectionExpression
    {
        private static readonly MethodInfo QueryableSelectMethod = FindQueryableSelectMethod();

        private readonly IQueryable _source;
        private readonly IMappingEngine _mappingEngine;

        public ProjectionExpression(IQueryable source, IMappingEngine mappingEngine)
        {
            _source = source;
            _mappingEngine = mappingEngine;
        }

        private static MethodInfo FindQueryableSelectMethod()
        {
            Expression<Func<IQueryable<object>>> select = () => Queryable.Select(default(IQueryable<object>), default(Expression<Func<object, object>>));
            MethodInfo method = ((MethodCallExpression)select.Body).Method.GetGenericMethodDefinition();
            return method;
        }

        public IQueryable<TResult> To<TResult>(object parameters = null)
        {
            return To<TResult>(parameters, new string[0]);
        }

        public IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand)
        {
            var paramValues = (parameters ?? new object()).GetType()
                .GetDeclaredProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));

            return To<TResult>(paramValues, membersToExpand);
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters)
        {
            return To<TResult>(parameters, new string[0]);
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters, params string[] membersToExpand)
        {
            var expr = _mappingEngine.CreateMapExpression(_source.ElementType, typeof(TResult), parameters, membersToExpand);

            return _source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    QueryableSelectMethod.MakeGenericMethod(_source.ElementType, typeof(TResult)),
                    new[] { _source.Expression, Expression.Quote(expr) }
                    )
                );
        }

        public IQueryable<TResult> To<TResult>(object parameters = null, params Expression<Func<TResult, object>>[] membersToExpand)
        {
            return To<TResult>(parameters, GetMemberNames(membersToExpand));
        }

        private string[] GetMemberNames<TResult>(Expression<Func<TResult, object>>[] membersToExpand)
        {
            return membersToExpand.Select(ReflectionHelper.GetPropertyName).ToArray();
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters, params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var memberNames = GetMemberNames(membersToExpand);
            var mapExpr = _mappingEngine.CreateMapExpression(_source.ElementType, typeof(TResult), parameters, memberNames);

            return _source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    QueryableSelectMethod.MakeGenericMethod(_source.ElementType, typeof(TResult)),
                    new[] { _source.Expression, Expression.Quote(mapExpr) }
                    )
                );
        }
    }
}
