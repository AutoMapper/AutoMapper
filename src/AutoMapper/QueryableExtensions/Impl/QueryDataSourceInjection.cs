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

        public QueryDataSourceInjection(IQueryable<TSource> dataSource)
        {
            _dataSource = dataSource;
        }

        public IQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null)
        {
            return new SourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(), Mapper.Engine, inspector);
        }
    }
}