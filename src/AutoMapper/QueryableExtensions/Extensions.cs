namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Impl;

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
        {
            return QueryMapperVisitor.Map<TSource, TDestination>(sourceQuery, destQuery, config);
        }

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource, IExpressionBuilder builder, IMapper mapper)
        {
            return new QueryDataSourceInjection<TSource>(dataSource, builder, mapper);
        }

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="builder">Expression builder</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IExpressionBuilder builder, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            return new ProjectionExpression(source, builder).To(parameters, membersToExpand);
        }

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            IExpressionBuilder builder,
            params Expression<Func<TDestination, object>>[] membersToExpand
            )
        {
            return source.ProjectTo(builder, null, membersToExpand);
        }

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TDestination">Destination type to map to</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="builder">Expression builder</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IExpressionBuilder builder, IDictionary<string, object> parameters, params string[] membersToExpand)
        {
            return new ProjectionExpression(source, builder).To<TDestination>(parameters, membersToExpand);
        }
    }
}
