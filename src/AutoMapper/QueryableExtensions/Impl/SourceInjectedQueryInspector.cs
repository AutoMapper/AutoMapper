using System;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class SourceInjectedQueryInspector
    {
        public SourceInjectedQueryInspector()
        {
            SourceResult = (e,o) => { };
            DestResult = o => { };
            StartQueryExecuteInterceptor = (t, e) => { };
        }
        public Action<Expression, object> SourceResult { get; set; }
        public Action<object> DestResult { get; set; }
        public Action<Type, Expression> StartQueryExecuteInterceptor { get; set; }

    }
}