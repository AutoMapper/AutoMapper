using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.QueryableExtensions
{
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
            => QueryMapperVisitor.Map(sourceQuery, destQuery, config);
        
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
            => new ProjectionExpression(source, configuration.ExpressionBuilder).To(parameters, membersToExpand);

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
            => new ProjectionExpression(source, configuration.ExpressionBuilder).To<TDestination>(parameters, membersToExpand);

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
            => new ProjectionExpression(source, configuration.ExpressionBuilder).To(destinationType, parameters, membersToExpand);            
    }
}
