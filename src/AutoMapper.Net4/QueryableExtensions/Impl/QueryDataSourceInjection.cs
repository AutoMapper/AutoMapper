using System.Linq;

namespace AutoMapper.QueryableExtensions.Impl
{
    public interface IQueryDataSourceInjection<TSource>
    {
        IQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null);
    }

    public class QueryDataSourceInjection<TSource> : IQueryDataSourceInjection<TSource>
    {
        private readonly IQueryable<TSource> _dataSource;
        private readonly IMappingEngine _mappingEngine;

        public QueryDataSourceInjection(IQueryable<TSource> dataSource, IMappingEngine mappingEngine)
        {
            _dataSource = dataSource;
            _mappingEngine = mappingEngine;
        }

        public IQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null)
        {
            return new SourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(), _mappingEngine, inspector);
        }
    }
}