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
        public static Func<IMapper> MapperServiceLocator;
         
        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mapper">Mapper</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(
            this IMapper mapper, IDictionary<string, object> parameters = null,
            params MemberInfo[] membersToExpand)
        {
            return
                (Expression<Func<TSource, TDestination>>)
                    mapper.CreateMapExpression(typeof(TSource), typeof(TDestination), parameters, membersToExpand);
        }

        /// <summary>
        /// Create an expression tree representing a mapping from the source type to destination type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <param name="mapper">Mapper</param>
        /// <param name="sourceType">Source Type</param>
        /// <param name="destinationType">Destination Type</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression CreateMapExpression(this IMapper mapper, Type sourceType, Type destinationType, IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand)
        {
            return mapper.CreateMapExpression(sourceType, destinationType, parameters, membersToExpand);
        }

        /// <summary>
        /// Maps a queryable expression of a source type to a queryable expression of a destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="sourceQuery">Source queryable</param>
        /// <param name="destQuery">Destination queryable</param>
        /// <returns>Mapped destination queryable</returns>
        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> sourceQuery,
            IQueryable<TDestination> destQuery)
        {
            if (MapperServiceLocator == null)
                throw new InvalidOperationException($"Set the {nameof(MapperServiceLocator)} function to provide a static instance of an W{nameof(IMapper)}");

            return sourceQuery.Map(destQuery, MapperServiceLocator());
        }

        /// <summary>
        /// Maps a queryable expression of a source type to a queryable expression of a destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="sourceQuery">Source queryable</param>
        /// <param name="destQuery">Destination queryable</param>
        /// <param name="mapper">Mapper</param>
        /// <returns>Mapped destination queryable</returns>
        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> sourceQuery, IQueryable<TDestination> destQuery, IMapper mapper)
        {
            return QueryMapperVisitor.Map<TSource, TDestination>(sourceQuery, destQuery, mapper);
        }

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource)
        {
            if (MapperServiceLocator == null)
                throw new InvalidOperationException($"Set the {nameof(MapperServiceLocator)} function to provide a static instance of an {nameof(IMapper)}");

            return dataSource.UseAsDataSource(MapperServiceLocator());
        }

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource, IMapper mapper)
        {
            return new QueryDataSourceInjection<TSource>(dataSource, mapper);
        }

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mapper">Mapper instance</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            object parameters,
            IMapper mapper,
            params Expression<Func<TDestination, object>>[] membersToExpand
            )
        {
            return new ProjectionExpression(source, mapper).To(parameters, membersToExpand);
        }

        /// <summary>
        /// Extension method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            object parameters,
            params Expression<Func<TDestination, object>>[] membersToExpand
            )
        {
            if (MapperServiceLocator == null)
                throw new InvalidOperationException($"Set the {nameof(MapperServiceLocator)} function to provide a static instance of an {nameof(IMapper)}");

            if (parameters is IMapper)
                throw new ArgumentException(
                    "Parameters is an IMapper. Use the overload that explicitly takes IMapper.",
                    nameof(parameters));

            return source.ProjectTo(parameters, MapperServiceLocator(), membersToExpand);
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
            params Expression<Func<TDestination, object>>[] membersToExpand
            )
        {
            return source.ProjectTo(null, membersToExpand);
        }

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TDestination">Destination type to map to</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="mapper"></param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IDictionary<string, object> parameters, IMapper mapper, params string[] membersToExpand)
        {
            return new ProjectionExpression(source, mapper).To<TDestination>(parameters, membersToExpand);
        }

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TDestination">Destination type to map to</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source,
            IDictionary<string, object> parameters,
            params string[] membersToExpand
            )
        {
            if (MapperServiceLocator == null)
                throw new InvalidOperationException($"Set the {nameof(MapperServiceLocator)} function to provide a static instance of an {nameof(IMapper)}");

            return ProjectTo<TDestination>(source, parameters, MapperServiceLocator(), membersToExpand);
        }
    }
}
