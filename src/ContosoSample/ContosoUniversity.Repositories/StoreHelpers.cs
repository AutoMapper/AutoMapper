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
using AutoMapper.XpressionMapper.Extensions;

namespace ContosoUniversity.Repositories
{
    internal static class StoreHelpers
    {
        internal static async Task<ICollection<TModel>> ModelQueryAsync<TModel, TData>(this IStore store, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Expression<Func<IQueryable<TData>, IQueryable<TData>>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc);
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            //Call the store
            ICollection<TData> list = await store.GetAsync(f,
                mappedQueryFunc == null ? null : mappedQueryFunc.Compile(),
                includes == null ? null : includes.Select(i => i.Compile()).ToList());

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(list).ToList();
        }

        internal static async Task<dynamic> QueryAsync<TModel, TData>(this IStore store, IMapper mapper,
            Expression<Func<IQueryable<TModel>, dynamic>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            //Map the expressions
            Expression<Func<IQueryable<TData>, dynamic>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, dynamic>>>(queryFunc);
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            //Call the store
            object result = await store.QueryAsync(mappedQueryFunc == null ? null : mappedQueryFunc.Compile(),
                includes == null ? null : includes.Select(i => i.Compile()).ToList());

            IEnumerable<TData> tDataList = result as IEnumerable<TData>;
            TData tData = result as TData;

            //Map and return the data
            return (tDataList) != null//Assign during type check so we don't have to cast twice.
                ? mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(tDataList).ToList()
                : (tData) != null//Assign during type check so we don't have to cast twice.
                         ? mapper.Map<TData, TModel>(tData)
                         : result;
        }

        internal static async Task<int> CountAsync<TModel, TData>(this IStore store, IMapper mapper, Expression<Func<TModel, bool>> filter = null, IDictionary<Type, Type> typeMappings = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            return await store.CountAsync(f);
        }

        internal static async Task<bool> SaveGraphsAsync<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData
        {
            //IList<TModel> eList = entities.ToList();
            IList<TData> items = mapper.Map<IEnumerable<TData>>(entities).ToList();
            bool success = await store.SaveGraphsAsync<TData>(items);

            IList<TModel> entityList = entities.ToList();
            for (int i = 0; i < items.Count; i++)
                mapper.Map<TData, TModel>(items[i], entityList[i]);

            return success;

            //Blows up with "Method 'Void Add(T)' declared on type 'System.Collections.Generic.ICollection`1[T]' cannot be called with instance of type 'System.Collections.Generic.IEnumerable`1[T]'"
            //mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(items, entities).ToList();
        }

        internal static async Task<bool> SaveAsync<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData
        {
            IList<TData> items = mapper.Map<IEnumerable<TData>>(entities).ToList();
            bool success = await store.SaveAsync<TData>(items);

            IList<TModel> entityList = entities.ToList();
            for (int i = 0; i < items.Count; i++)
                mapper.Map<TData, TModel>(items[i], entityList[i]);

            return success;
            //Blows up with "Method 'Void Add(T)' declared on type 'System.Collections.Generic.ICollection`1[T]' cannot be called with instance of type 'System.Collections.Generic.IEnumerable`1[T]'"
            //mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(items, entities).ToList();
        }

        internal static async Task<bool> DeleteAsync<TModel, TData>(this IStore store, IMapper mapper, Expression<Func<TModel, bool>> filter = null, IDictionary<Type, Type> typeMappings = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            List<TData> list = (await store.GetAsync(f)).ToList();
            list.ForEach(item => { item.EntityState = ContosoUniversity.Data.EntityStateType.Deleted; });
            return await store.SaveAsync<TData>(list);
        }

        internal static void AddChanges<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData
        {
            store.AddChanges<TData>(mapper.Map<IEnumerable<TData>>(entities).ToList());
        }

        internal static void AddGraphChanges<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : BaseModel
            where TData : BaseData
        {
            store.AddGraphChanges<TData>(mapper.Map<IEnumerable<TData>>(entities).ToList());
        }
    }
}
