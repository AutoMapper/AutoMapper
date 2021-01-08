using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Impl;
namespace AutoMapper.QueryableExtensions
{
    using MemberPaths = IEnumerable<MemberInfo[]>;
    using ParameterBag = IDictionary<string, object>;
    /// <summary>
    /// Queryable extensions for AutoMapper
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Maps a queryable expression of a source type to a queryable expression of a destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="sourceQuery">Source queryable</param>
        /// <param name="destQuery">Destination queryable</param>
        /// <param name="config"></param>
        /// <returns>Mapped destination queryable</returns>
        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> sourceQuery, IQueryable<TDestination> destQuery, IConfigurationProvider config) 
            => QueryMapperVisitor.Map(sourceQuery, destQuery, config.Internal());
        
        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="configuration">Mapper configuration</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IConfigurationProvider configuration, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand) 
            => new ProjectionExpression(source, configuration).To(parameters, membersToExpand);

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="configuration">Mapper configuration</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            IConfigurationProvider configuration,
            params Expression<Func<TDestination, object>>[] membersToExpand
            )
            => source.ProjectTo(configuration, null, membersToExpand);

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TDestination">Destination type to map to</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="configuration">Mapper configuration</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IConfigurationProvider configuration, IDictionary<string, object> parameters, params string[] membersToExpand) 
            => new ProjectionExpression(source, configuration).To<TDestination>(parameters, membersToExpand);

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <param name="source">Queryable source</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="configuration">Mapper configuration</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable ProjectTo(this IQueryable source, Type destinationType, IConfigurationProvider configuration)
            => source.ProjectTo(destinationType, configuration, null);

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <param name="source">Queryable source</param>
        /// <param name="destinationType">Destination type to map to</param>
        /// <param name="configuration">Mapper configuration</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        public static IQueryable ProjectTo(this IQueryable source, Type destinationType, IConfigurationProvider configuration, IDictionary<string, object> parameters, params string[] membersToExpand) 
            => new ProjectionExpression(source, configuration).To(destinationType, parameters, membersToExpand);
        readonly struct ProjectionExpression
        {
            private static readonly MethodInfo SelectMethod = typeof(Queryable).StaticGenericMethod("Select", parametersCount: 2);
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
                var members = memberPathsToExpand.Select(m => new MemberPath(m)).ToArray();
                return _builder.GetProjection(_source.ElementType, destinationType, parameters, members).Chain(_source, Select);
            }

            private static IQueryable Select(IQueryable source, LambdaExpression lambda) => source.Provider.CreateQuery(
                Expression.Call(SelectMethod.MakeGenericMethod(source.ElementType, lambda.ReturnType), source.Expression, Expression.Quote(lambda)));
        }
    }
    public class MemberVisitor : ExpressionVisitor
    {
        public static MemberInfo[] GetMemberPath(Expression expression)
        {
            var memberVisitor = new MemberVisitor();
            memberVisitor.Visit(expression);
            return memberVisitor.MemberPath.ToArray();
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            _members.AddRange(node.GetMemberExpressions().Select(e => e.Member));
            return node;
        }
        private readonly List<MemberInfo> _members = new List<MemberInfo>();
        public IReadOnlyCollection<MemberInfo> MemberPath => _members;
    }
}