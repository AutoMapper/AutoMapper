using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using MemberPaths = IEnumerable<MemberInfo[]>;
    using ParameterBag = IDictionary<string, object>;

    public readonly struct ProjectionExpression
    {
        private readonly IQueryable _source;
        private readonly IProjectionBuilder _builder;

        public ProjectionExpression(IQueryable source, IConfigurationProvider configuration)
        {
            _source = source;
            _builder = configuration.Internal().ProjectionBuilder;
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
            var members = memberPathsToExpand.Select(m=>new MemberPath(m)).ToArray();
            return _builder.GetProjection(_source.ElementType, destinationType, parameters, members).Chain(_source, Select);
        }

        private static IQueryable Select(IQueryable source, LambdaExpression lambda) => source.Provider.CreateQuery(
            Expression.Call(typeof(Queryable),"Select", new[] { source.ElementType, lambda.ReturnType }, source.Expression, Expression.Quote(lambda)));
    }
}