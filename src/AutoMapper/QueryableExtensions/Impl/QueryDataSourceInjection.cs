using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using MemberPaths = IEnumerable<IEnumerable<MemberInfo>>;
    using IObjectDictionary = IDictionary<string, object>;

    public interface IQueryDataSourceInjection<TSource>
    {
        /// <summary>
        /// Creates the mapped query with an optional inspector
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <returns></returns>
        ISourceInjectedQueryable<TDestination> For<TDestination>();
        ISourceInjectedQueryable<TDestination> For<TDestination>(object parameters, params Expression<Func<TDestination, object>>[] membersToExpand);
        ISourceInjectedQueryable<TDestination> For<TDestination>(params Expression<Func<TDestination, object>>[] membersToExpand);
        ISourceInjectedQueryable<TDestination> For<TDestination>(IObjectDictionary parameters, params string[] membersToExpand);

        IQueryDataSourceInjection<TSource> UsingInspector(SourceInjectedQueryInspector inspector);

        /// <summary>
        /// ExpressionVisitors called before MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> BeforeProjection(params ExpressionVisitor[] visitors);

        /// <summary>
        /// ExpressionVisitors called after the MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> AfterProjection(params ExpressionVisitor[] visitors);

        /// <summary>
        /// Allows specifying a handler that will be called when the underlying QueryProvider encounters an exception.
        /// This is especially useful if you expose the resulting IQueryable in e.g. a WebApi controller where
        /// you do not call "ToList" yourself and therefore cannot catch exceptions
        /// </summary>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> OnError(Action<Exception> exceptionHandler);
    }

    public class QueryDataSourceInjection<TSource> : IQueryDataSourceInjection<TSource>
    {
        private readonly IQueryable<TSource> _dataSource;
        private readonly IMapper _mapper;
        private readonly List<ExpressionVisitor> _beforeMappingVisitors = new List<ExpressionVisitor>();
        private readonly List<ExpressionVisitor> _afterMappingVisitors = new List<ExpressionVisitor>();
        private readonly ExpressionVisitor _sourceExpressionTracer = null;
        private readonly ExpressionVisitor _destinationExpressionTracer = null;
        private Action<Exception> _exceptionHandler = x => { };
        private MemberPaths _membersToExpand;
        private IObjectDictionary _parameters;
        private SourceInjectedQueryInspector _inspector;

        public QueryDataSourceInjection(IQueryable<TSource> dataSource, IMapper mapper)
        {
            _dataSource = dataSource;
            _mapper = mapper;
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>() => CreateQueryable<TDestination>();

        public ISourceInjectedQueryable<TDestination> For<TDestination>(object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            _parameters = GetParameters(parameters);
            _membersToExpand = ProjectionExpression.GetMemberPaths(membersToExpand);
            return CreateQueryable<TDestination>();
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>(params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            _membersToExpand = ProjectionExpression.GetMemberPaths(membersToExpand);
            return CreateQueryable<TDestination>();
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>(IObjectDictionary parameters, params string[] membersToExpand)
        {
            _parameters = parameters;
            _membersToExpand = ProjectionExpression.GetMemberPaths(typeof(TDestination), membersToExpand);
            return CreateQueryable<TDestination>();
        }

        public IQueryDataSourceInjection<TSource> UsingInspector(SourceInjectedQueryInspector inspector)
        {
            _inspector = inspector;

            if (_sourceExpressionTracer != null)
                _beforeMappingVisitors.Insert(0, _sourceExpressionTracer);
            if (_destinationExpressionTracer != null)
                _afterMappingVisitors.Add(_destinationExpressionTracer);

            return this;
        }

        /// <summary>
        /// ExpressionVisitors called before MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> BeforeProjection(params ExpressionVisitor[] visitors)
        {
            foreach (var visitor in visitors.Where(visitor => !_beforeMappingVisitors.Contains(visitor)))
            {
                _beforeMappingVisitors.Add(visitor);
            }
            return this;
        }

        /// <summary>
        /// ExpressionVisitors called after the MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> AfterProjection(params ExpressionVisitor[] visitors)
        {
            foreach (var visitor in visitors.Where(visitor => !_afterMappingVisitors.Contains(visitor)))
            {
                _afterMappingVisitors.Add(visitor);
            }
            return this;
        }

        /// <summary>
        /// Allows specifying a handler that will be called when the underlying QueryProvider encounters an exception.
        /// This is especially useful if you expose the resulting IQueryable in e.g. a WebApi controller where
        /// you do not call "ToList" yourself and therefore cannot catch exceptions
        /// </summary>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> OnError(Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            return this;
        }

        private ISourceInjectedQueryable<TDestination> CreateQueryable<TDestination>() =>
            new SourceSourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(),
                _mapper,
                _beforeMappingVisitors,
                _afterMappingVisitors,
                _exceptionHandler,
                _parameters,
                _membersToExpand,
                _inspector);

        private static IObjectDictionary GetParameters(object parameters)
        {
            return (parameters ?? new object()).GetType()
                .GetDeclaredProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));
        }
    }
}