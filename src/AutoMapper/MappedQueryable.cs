using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper
{
    public class MappedQueryable<TDestination, TSource> : IOrderedQueryable<TDestination>
    {
        private Expression _expression;
        private MappedQueryProvider<TSource> _provider;

        internal MappedQueryable(
           MappedQueryProvider<TSource> provider,
           Expression expression)
        {
            this._provider = provider;
            this._expression = expression;
        }
        public IEnumerator<TDestination> GetEnumerator()
        {
            return this._provider.ExecuteQuery<TDestination>(this._expression);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._provider.ExecuteQuery<TDestination>(this._expression);
        }

        public Type ElementType
        {
            get { return typeof(TDestination); }
        }
        public Expression Expression
        {
            get { return this._expression; }
        }
        public IQueryProvider Provider
        {
            get { return this._provider; }
        }
    }
}
