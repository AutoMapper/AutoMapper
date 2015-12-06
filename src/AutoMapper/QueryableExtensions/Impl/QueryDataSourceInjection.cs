﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Visitors;
using IObjectDictionary = System.Collections.Generic.IDictionary<string, object>;

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
        /// adds an ExpressionVisitor that will trace the source expression.
        /// </summary>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> TraceSourceExpressionTo(TextWriter output = null);

        /// <summary>
        /// adds an ExpressionVisitor that will trace the destination expression.
        /// </summary>
        /// <returns></returns>
        IQueryDataSourceInjection<TSource> TraceDestinationExpressionTo(TextWriter output = null);

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
        private readonly IMappingEngine _mappingEngine;
        private readonly List<ExpressionVisitor> _beforeMappingVisitors = new List<ExpressionVisitor>();
        private readonly List<ExpressionVisitor> _afterMappingVisitors = new List<ExpressionVisitor>();
        private ExpressionVisitor _sourceExpressionTracer = null;
        private ExpressionVisitor _destinationExpressionTracer = null;
        private Action<Exception> _exceptionHandler = ((x) => { });
        private string[] _membersToExpand = null;
        private IObjectDictionary _parameters = null;
        private SourceInjectedQueryInspector _inspector;

        public QueryDataSourceInjection(IQueryable<TSource> dataSource, IMappingEngine mappingEngine)
        {
            _dataSource = dataSource;
            _mappingEngine = mappingEngine;
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>()
        {
            return CreateQueryable<TDestination>();
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>(object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            return CreateQueryable<TDestination>(membersToExpand);
        }
        
        public ISourceInjectedQueryable<TDestination> For<TDestination>(params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            return CreateQueryable<TDestination>(membersToExpand);
        }

        public ISourceInjectedQueryable<TDestination> For<TDestination>(IObjectDictionary parameters, params string[] membersToExpand)
        {
            _parameters = parameters;
            _membersToExpand = membersToExpand;
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
        public IQueryDataSourceInjection<TSource> AfterProjection(params ExpressionVisitor[] visitors)
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
        public IQueryDataSourceInjection<TSource> TraceSourceExpressionTo(TextWriter output = null)
        {
            output = output ?? Console.Out;
            _sourceExpressionTracer = new ExpressionWriter(output);
            return this;
        }

        /// <summary>
        /// adds an ExpressionVisitor that will trace the destination expression.
        /// </summary>
        /// <returns></returns>
        public IQueryDataSourceInjection<TSource> TraceDestinationExpressionTo(TextWriter output = null)
        {
            output = output ?? Console.Out;
            _destinationExpressionTracer = new ExpressionWriter(output);
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
        
        private ISourceInjectedQueryable<TDestination> CreateQueryable<TDestination>(params Expression<Func<TDestination, object>>[] memberExpressionsToExpand)
        {
            return new SourceSourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(),
                _mappingEngine,
                _beforeMappingVisitors,
                _afterMappingVisitors,
                _exceptionHandler,
                _parameters,
                _membersToExpand,
                memberExpressionsToExpand,
                _inspector);
        }

        private static IObjectDictionary GetParameters(object parameters)
        {
            return (parameters ?? new object()).GetType()
                .GetDeclaredProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));
        }
    }
}