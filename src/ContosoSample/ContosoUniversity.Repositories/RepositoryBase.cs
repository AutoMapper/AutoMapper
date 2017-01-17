using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using ContosoUniversity.Crud.DataStores;
using ContosoUniversity.Data;
using ContosoUniversity.Domain;

namespace ContosoUniversity.Repositories
{
    abstract public class RepositoryBase<TModel, TData> : IRepositoryBase<TModel, TData>
        where TModel : BaseModel
            where TData : BaseData
    {
        public RepositoryBase(IStore store, IMapper mapper)
        {
            this._store = store;
            this._mapper = mapper;
        }

        #region Fields
        private IStore _store;
        private IMapper _mapper;
        #endregion Fields

        #region Methods
        public async Task<ICollection<TModel>> GetItemsAsync(Expression<Func<TModel, bool>> filter = null, Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null, ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            return await _store.ModelQueryAsync<TModel, TData>(
                _mapper,
                filter,
                queryFunc,
                includeProperties);
        }

        public async Task<int> CountAsync(Expression<Func<TModel, bool>> filter = null)
        {
            return await _store.CountAsync<TModel, TData>(_mapper, filter);
        }

        public async Task<dynamic> QueryAsync(Expression<Func<IQueryable<TModel>, dynamic>> queryFunc, ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            return await _store.QueryAsync<TModel, TData>(
                _mapper,
                queryFunc,
                includeProperties);
        }

        public async Task<bool> SaveAsync(TModel entity)
        {
            return await _store.SaveAsync<TModel, TData>(_mapper, new List<TModel> { entity });
        }

        public async Task<bool> SaveAsync(ICollection<TModel> entities)
        {
            return await _store.SaveAsync<TModel, TData>(_mapper, entities);
        }

        public async Task<bool> SaveGraphAsync(TModel entity)
        {
            return await _store.SaveGraphsAsync<TModel, TData>(_mapper, new List<TModel> { entity });
        }

        public async Task<bool> SaveGraphsAsync(ICollection<TModel> entities)
        {
            return await _store.SaveGraphsAsync<TModel, TData>(_mapper, entities);
        }

        public async Task<bool> DeleteAsync(Expression<Func<TModel, bool>> filter = null)
        {
            return await _store.DeleteAsync<TModel, TData>(_mapper, filter);
        }

        public void AddChange(TModel entity)
        {
            _store.AddChanges<TModel, TData>(_mapper, new List<TModel> { entity });
        }

        public void AddChanges(ICollection<TModel> entities)
        {
            _store.AddChanges<TModel, TData>(_mapper, entities);
        }

        public void AddGraphChange(TModel entity)
        {
            _store.AddGraphChanges<TModel, TData>(_mapper, new List<TModel> { entity });
        }

        public void AddGraphChanges(ICollection<TModel> entities)
        {
            _store.AddGraphChanges<TModel, TData>(_mapper, entities);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _store.SaveChangesAsync();
        }
        #endregion Methods
    }
}
