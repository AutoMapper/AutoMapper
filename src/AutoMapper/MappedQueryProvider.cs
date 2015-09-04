using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    /// <summary>
    /// Provides an interceptable IQueryable
    /// Copied from http://blogs.msdn.com/b/alexj/archive/2010/03/01/tip-55-how-to-extend-an-iqueryable-by-wrapping-it.aspx
    /// </summary>
    public class MappedQueryProvider<TSource> : IQueryProvider
    {
        private readonly IQueryable _underlyingQuery;
        private readonly IMappingEngine _engine;
        private IQueryProvider _underlyingProvider;

        private MappedQueryProvider(IQueryable underlyingQuery, IMappingEngine engine)
        {
            _underlyingQuery = underlyingQuery;
            _engine = engine;
            this._underlyingProvider = underlyingQuery.Provider;
        }

        public static IQueryable<T> Map<T>(IQueryable<TSource> underlyingQuery, IMappingEngine engine)
        {
            return new MappedQueryable<T, TSource>(new MappedQueryProvider<TSource>(underlyingQuery, engine), new T[0].AsQueryable().Expression);
        }

        public IEnumerator<TElement> ExecuteQuery<TElement>(Expression expression)
        {
            var visitor = new QueryMapperVisitor(typeof(TElement), typeof(TSource), _underlyingQuery, _engine);
            var expr = visitor.Visit(expression);

            var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);
            
            return newDestQuery.ProjectTo<TElement>().GetEnumerator();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MappedQueryable<TElement, TSource>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type et = TypeHelper.FindIEnumerable(expression.Type);
            Type dt = typeof (TSource);
            Type qt = typeof(MappedQueryable<,>).MakeGenericType(et, dt);
            object[] args = new object[] { this, expression };

            ConstructorInfo ci = qt.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] {
                typeof(MappedQueryProvider<TSource>),
                typeof(Expression)
            },
                null);

            //return (IQueryable)ci.Invoke(args);
            throw new NotSupportedException();
        }
        public TResult Execute<TResult>(Expression expression)
        {
            //return this._underlyingProvider.Execute<TResult>(InterceptExpr(expression));

            var visitor = new QueryMapperVisitor(typeof(TSource), typeof(TResult), _underlyingQuery, _engine);
            var expr = visitor.Visit(expression);

            var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);

            return newDestQuery.ProjectTo<TResult>().SingleOrDefault();
        }
        public object Execute(Expression expression)
        {
            throw new NotSupportedException();
            
            
            //return this._underlyingProvider.Execute(InterceptExpr(expression));
            
            
            //var visitor = new QueryMapperVisitor(typeof(TSource), typeof(object), _underlyingQuery, _engine);
            //var expr = visitor.Visit(expression);
            //var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);
            //return newDestQuery.SingleOrDefault();
        }
    }
}
