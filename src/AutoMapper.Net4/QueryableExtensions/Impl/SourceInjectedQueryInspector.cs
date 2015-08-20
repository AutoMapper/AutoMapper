namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq.Expressions;

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