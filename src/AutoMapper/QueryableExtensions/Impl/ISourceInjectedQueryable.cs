using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.QueryableExtensions.Impl
{
    public interface ISourceInjectedQueryable<T> : IQueryable<T>
    {
        /// <summary>
        /// Called when [enumerated].
        /// </summary>
        /// <param name="enumerationHandler">The enumeration handler.</param>
        IQueryable<T> OnEnumerated(Action<IEnumerable<object>> enumerationHandler);
    }
}