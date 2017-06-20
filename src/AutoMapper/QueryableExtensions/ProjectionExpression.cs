using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using IObjectDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.QueryableExtensions
{
    using MemberPaths = IEnumerable<IEnumerable<MemberInfo>>;

    public class ProjectionExpression : IProjectionExpression
    {
        private static readonly MethodInfo QueryableSelectMethod = FindQueryableSelectMethod();

        private readonly IQueryable _source;
        private readonly IExpressionBuilder _builder;

        public ProjectionExpression(IQueryable source, IExpressionBuilder builder)
        {
            _source = source;
            _builder = builder;
        }

        private static MethodInfo FindQueryableSelectMethod()
        {
            Expression<Func<IQueryable<object>>> select = () => default(IQueryable<object>).Select(default(Expression<Func<object, object>>));
            var method = ((MethodCallExpression)select.Body).Method.GetGenericMethodDefinition();
            return method;
        }

        public IQueryable<TResult> To<TResult>(object parameters = null) => To<TResult>(parameters, new string[0]);

        public IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand)
        {
            var paramValues = GetParameters(parameters);
            return To<TResult>(paramValues, membersToExpand);
        }

        private static IObjectDictionary GetParameters(object parameters)
        {
            return (parameters ?? new object()).GetType()
                .GetDeclaredProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));
        }

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters) => To<TResult>(parameters, new string[0]);

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters, params string[] membersToExpand)
        {
            var members = GetMemberPaths(typeof(TResult), membersToExpand);
            return To<TResult>(parameters, members);
        }

        public IQueryable<TResult> To<TResult>(object parameters = null, params Expression<Func<TResult, object>>[] membersToExpand) 
            => To<TResult>(GetParameters(parameters), GetMemberPaths(membersToExpand));

        public static MemberPaths GetMemberPaths(Type type, string[] membersToExpand) =>
            membersToExpand.Select(m => ReflectionHelper.GetMemberPath(type, m));

        public static MemberPaths GetMemberPaths<TResult>(Expression<Func<TResult, object>>[] membersToExpand) =>
            membersToExpand.Select(expr => MemberVisitor.GetMemberPath(expr));

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters, params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = GetMemberPaths(membersToExpand);
            return To<TResult>(parameters, members);
        }

        internal IQueryable<TResult> To<TResult>(IObjectDictionary parameters, MemberPaths memberPathsToExpand)
        {
            var membersToExpand = memberPathsToExpand.SelectMany(m => m).Distinct().ToArray();

            parameters = parameters ?? new Dictionary<string, object>();
            var mapExpressions = _builder.GetMapExpression(_source.ElementType, typeof(TResult), parameters, membersToExpand);

            return (IQueryable<TResult>)mapExpressions.Aggregate(_source, (source, lambda)=>Select(source, lambda));
        }

        private static IQueryable Select(IQueryable source, LambdaExpression lambda)
        {
            return source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableSelectMethod.MakeGenericMethod(source.ElementType, lambda.ReturnType),
                    new[] { source.Expression, Expression.Quote(lambda) }
                    )
                );
        }
    }
}