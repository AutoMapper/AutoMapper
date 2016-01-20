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
        private readonly IMapper _mapper;

        public QueryDataSourceInjection(IQueryable<TSource> dataSource, IMapper mapper)
        {
            _dataSource = dataSource;
            _mapper = mapper;
        }

        public IQueryable<TDestination> For<TDestination>(SourceInjectedQueryInspector inspector = null)
        {
            return new SourceInjectedQuery<TSource, TDestination>(_dataSource,
                new TDestination[0].AsQueryable(), _mapper, inspector);
        }
    }
}