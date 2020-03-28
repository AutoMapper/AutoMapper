using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions
{
    using MemberPaths = IEnumerable<IEnumerable<MemberInfo>>;
    using ParameterBag = IDictionary<string, object>;

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

        public IQueryable<TResult> To<TResult>(ParameterBag parameters, string[] membersToExpand) =>
            ToCore<TResult>(parameters, membersToExpand.Select(memberName => ReflectionHelper.GetMemberPath(typeof(TResult), memberName)));

        public IQueryable<TResult> To<TResult>(object parameters, Expression<Func<TResult, object>>[] membersToExpand) =>
            ToCore<TResult>(parameters, membersToExpand.Select(MemberVisitor.GetMemberPath));

        public IQueryable To(Type destinationType, object parameters, string[] membersToExpand) =>
            ToCore(destinationType, parameters, membersToExpand.Select(memberName => ReflectionHelper.GetMemberPath(destinationType, memberName)));

        private IQueryable<TResult> ToCore<TResult>(object parameters, MemberPaths memberPathsToExpand) =>
            (IQueryable<TResult>)ToCore(typeof(TResult), parameters, memberPathsToExpand);

        private IQueryable ToCore(Type destinationType, object parameters, MemberPaths memberPathsToExpand)
        {
            var members = memberPathsToExpand.SelectMany(m => m).Distinct().ToArray();
            return _builder.GetMapExpression(_source.ElementType, destinationType, parameters, members).Aggregate(_source, Select);
        }

        private static IQueryable Select(IQueryable source, LambdaExpression lambda) => source.Provider.CreateQuery(
                Expression.Call(
                    null,
                    QueryableSelectMethod.MakeGenericMethod(source.ElementType, lambda.ReturnType),
                    new[] { source.Expression, Expression.Quote(lambda) }
                    )
                );

        private static MethodInfo FindQueryableSelectMethod()
        {
            Expression<Func<IQueryable<object>>> select = () => default(IQueryable<object>).Select(default(Expression<Func<object, object>>));
            var method = ((MethodCallExpression)select.Body).Method.GetGenericMethodDefinition();
            return method;
        }
    }
}