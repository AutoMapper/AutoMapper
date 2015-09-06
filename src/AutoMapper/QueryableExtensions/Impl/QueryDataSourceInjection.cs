using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;

namespace AutoMapper.QueryableExtensions.Impl
{
    public interface IQueryDataSourceInjection<TSource>
    {
        /// <summary>
        /// Creates the mapped query with an optional inspector
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="inspector">The inspector.</param>
        /// <returns></returns>
        ISourceInjectedQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null);

        /// <summary>
        /// ExpressionVisitors called before MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> VisitBeforeMapping(params ExpressionVisitor[] visitors);

        /// <summary>
        /// ExpressionVisitors called after the MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> VisitAfterMapping(params ExpressionVisitor[] visitors);

        /// <summary>
        /// adds an ExpressionVisitor that will trace the source expression.
        /// </summary>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> TraceSourceExpression();

        /// <summary>
        /// adds an ExpressionVisitor that will trace the destination expression.
        /// </summary>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> TraceDestinationExpression();

        /// <summary>
        /// Allows specifying a handler that will be called when the underlying QueryProvider encounters an exception.
        /// This is especially useful if you expose the resulting IQueryable in e.g. a WebApi controller where
        /// you do not call "ToList" yourself and therefore cannot catch exceptions
        /// </summary>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> OnError(Action<Exception> exceptionHandler);
    }

    //
    // Summary:
    //     Stellt Funktionen zur Auswertung von Abfragen für eine spezifische Datenquelle
    //     mit unbekanntem Datentyp bereit.
    //
    // Type parameters:
    //   T:
    //     Der Datentyp in der Datenquelle.

    public class QueryDataSourceInjection<TSource> : IQueryDataSourceInjection<TSource>
    {
        private readonly IQueryable<TSource> _dataSource;
        private readonly IMapper _mapper;
        private readonly List<ExpressionVisitor> _beforeMappingVisitors = new List<ExpressionVisitor>();
        private readonly List<ExpressionVisitor> _afterMappingVisitors = new List<ExpressionVisitor>();
        private Action<Exception> _exceptionHandler = ((x) => { });

        public QueryDataSourceInjection(IQueryable<TSource> dataSource, IMapper mapper)
        {
            _dataSource = dataSource;
            _mapper = mapper;
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null)
        {
            return new SourceSourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(), _mapper, _beforeMappingVisitors, _afterMappingVisitors, _exceptionHandler, inspector);
        }

        /// <summary>
        /// ExpressionVisitors called before MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> VisitBeforeMapping(params ExpressionVisitor[] visitors)
        {
            foreach(var visitor in visitors)
            {
                if (!_beforeMappingVisitors.Contains(visitor))
                    _beforeMappingVisitors.Add(visitor);
            }
            return this;
        }

        /// <summary>
        /// ExpressionVisitors called after the MappingVisitor itself is executed
        /// </summary>
        /// <param name="visitors">The visitors.</param>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> VisitAfterMapping(params ExpressionVisitor[] visitors)
        {
            foreach (var visitor in visitors)
            {
                if (!_afterMappingVisitors.Contains(visitor))
                    _afterMappingVisitors.Add(visitor);
            }
            return this;
        }

        /// <summary>
        /// adds an ExpressionVisitor that will trace the source expression.
        /// </summary>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> TraceSourceExpression()
        {
            return this;
        }

        /// <summary>
        /// adds an ExpressionVisitor that will trace the destination expression.
        /// </summary>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> TraceDestinationExpression()
        {
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
    }
}
