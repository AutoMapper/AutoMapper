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

    public class ProjectionExpression
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

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters, params string[] membersToExpand) =>
            To<TResult>(parameters, GetMemberPaths(typeof(TResult), membersToExpand));

        public IQueryable<TResult> To<TResult>(object parameters, params Expression<Func<TResult, object>>[] membersToExpand) =>
            ToCore<TResult>(parameters, GetMembers(GetMemberPaths(membersToExpand)));

        public static MemberPaths GetMemberPaths(Type type, string[] membersToExpand) =>
            membersToExpand.Select(m => ReflectionHelper.GetMemberPath(type, m));

        public static MemberPaths GetMemberPaths<TResult>(Expression<Func<TResult, object>>[] membersToExpand) =>
            membersToExpand.Select(expr => MemberVisitor.GetMemberPath(expr));

        public static MemberInfo[] GetMembers(MemberPaths memberPathsToExpand) =>
           memberPathsToExpand.SelectMany(m => m).Distinct().ToArray();

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters, params Expression<Func<TResult, object>>[] membersToExpand) =>
            To<TResult>(parameters, GetMemberPaths(membersToExpand));

        public IQueryable<TResult> To<TResult>(IObjectDictionary parameters, MemberPaths memberPathsToExpand) =>
            ToCore<TResult>(parameters, GetMembers(memberPathsToExpand));

        private IQueryable<TResult> ToCore<TResult>(object parameters, MemberInfo[] members)=>
            (IQueryable<TResult>)_builder.GetMapExpression(_source.ElementType, typeof(TResult), parameters, members).Aggregate(_source, Select);

        private static IQueryable Select(IQueryable source, LambdaExpression lambda) => source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableSelectMethod.MakeGenericMethod(source.ElementType, lambda.ReturnType),
                    new[] { source.Expression, Expression.Quote(lambda) }
                    )
                );
    }
}