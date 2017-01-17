using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using ContosoUniversity.Data;
using ContosoUniversity.Domain;

namespace ContosoUniversity.Repositories
{
    public interface IRepository
    {
        Task<ICollection<TModel>> GetItemsAsync<TModel, TData>(Expression<Func<TModel, bool>> filter = null, Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null, ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            where TModel : BaseModel
            where TData : BaseData;

        Task<int> CountAsync<TModel, TData>(Expression<Func<TModel, bool>> filter = null)
            where TModel : BaseModel
            where TData : BaseData;

        Task<dynamic> QueryAsync<TModel, TData>(Expression<Func<IQueryable<TModel>, dynamic>> queryFunc, ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> SaveAsync<TModel, TData>(TModel entity)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> SaveAsync<TModel, TData>(ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> SaveGraphAsync<TModel, TData>(TModel entity)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> SaveGraphsAsync<TModel, TData>(ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> DeleteAsync<TModel, TData>(System.Linq.Expressions.Expression<Func<TModel, bool>> filter = null)
            where TModel : BaseModel
            where TData : BaseData;

        void AddChange<TModel, TData>(TModel entity)
            where TModel : BaseModel
            where TData : BaseData;

        void AddChanges<TModel, TData>(ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData;

        void AddGraphChange<TModel, TData>(TModel entity)
            where TModel : BaseModel
            where TData : BaseData;

        void AddGraphChanges<TModel, TData>(ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData;

        Task<bool> SaveChangesAsync();
    }
}
