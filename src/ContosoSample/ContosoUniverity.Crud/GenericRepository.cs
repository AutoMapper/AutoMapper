using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ContosoUniversity.Crud
{
    public class GenericRepository<T> where T : class
    {
        public GenericRepository(DbContext context)
        {
            this.context = context;
            this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.context.ChangeTracker.AutoDetectChangesEnabled = false;
            this.dbSet = context.Set<T>();
        }

        #region Fields
        private DbContext context;
        private DbSet<T> dbSet;
        #endregion Fields

        #region Properties
        public DbSet<T> DbSet { get { return this.dbSet; } }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Expression<Func<T,bool>> filter e.g. student => student.Name == smith
        /// Func<IQueryable<T>, IQueryable<T>> orderBy e.g. q => q.OrderBy(s => s.name)
        /// ICollection<Expression<Func<T, object>>> includeProperties e.g. new ICollection<Expression<Func<T, object>>> { user => user.Address, user => user.Roles }
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual async Task<ICollection<T>> GetAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IQueryable<T>> queryableFunc = null, ICollection<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includeProperties = null)
        {//public virtual ICollection<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IQueryable<T>> queryableFunc = null, ICollection<Expression<Func<T, object>>> includeProperties = null)
            IQueryable<T> query = this.dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (includeProperties != null)
                query = includeProperties.Aggregate(query, (list, next) => query = next(query));
            //query = includeProperties.Aggregate(query, (q, inc) => q.Include(inc));

            return queryableFunc != null ? await queryableFunc(query).ToListAsync() : await query.ToListAsync();
        }

        /// <summary>
        /// General query function to return lists or scalar value.
        /// </summary>
        /// <param name="queryableFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual async Task<dynamic> QueryAsync(Func<IQueryable<T>, dynamic> queryableFunc, ICollection<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includeProperties = null)
        {
            IQueryable<T> query = this.dbSet;

            if (includeProperties != null)
                query = includeProperties.Aggregate(query, (list, next) => query = next(query));

            dynamic returnValue = queryableFunc(query);
            Type returnType = returnValue.GetType();

            return returnType.GetInterfaces().Any(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryable<>))
                ? await EntityFrameworkQueryableExtensions.ToListAsync(returnValue)
                : returnValue;
        }

        /// <summary>
        /// Returns a count of all rows to be returned by the query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = this.dbSet;

            if (filter != null)
                query = query.Where(filter);

            return await query.CountAsync();
        }

        /// <summary>
        /// Inserts an object graph into the database
        /// </summary>
        /// <param name="t"></param>
        public virtual void InsertGraph(T t)
        {
            //List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> before = context.ChangeTracker.Entries<Data.BaseData>().ToList();
            this.dbSet.Add(t);
            //List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> after = context.ChangeTracker.Entries<Data.BaseData>().ToList();
            //Dump(after);
        }

        private static void Dump(List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> entries)
        {
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData> entry in entries)
                System.Diagnostics.Debug.WriteLine("Type: {0}, State: {1}.", entry.Entity.GetType().Name, entry.State.ToString());
        }

        /// <summary>
        /// Inserts only the root object - even if there are child objects attached.
        /// </summary>
        /// <param name="t"></param>
        public virtual void Insert(T t)
        {
            /*When more than one item is being added this blows up with:
             * The instance of entity type 'T' cannot be tracked because another instance of this type with the same key is already being tracked. When adding new entities, for most key types a unique temporary key value will be created if no key is set (i.e. if the key property is assigned the default value for its type). If you are explicitly setting key values for new entities, ensure they do not collide with existing entities or temporary values generated for other new entities. When attaching existing entities, ensure that only one entity instance with a given key value is attached to the context.
             */
            //this.dbSet.Attach(t);
            //Important to attach first otherwise the statement below behaves like as add (this.dbSet.Add(t);) - NOT TRUE IN EF CORE 1.1
            //Fortunately this is mo longer true in EF Core and the code below works (it only tracks the root - no child objects will be tracked)

            //List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> before = context.ChangeTracker.Entries<Data.BaseData>().ToList();
            this.context.Entry(t).State = EntityState.Added;//Set only the root to Added.
            //List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> after = context.ChangeTracker.Entries<Data.BaseData>().ToList();
            //Dump(after);
        }

        /// <summary>
        /// Deletes the entity - deafult behavior is Cascade on child objects.  Do we need a DeleteGraph function.
        /// </summary>
        /// <param name="t"></param>
        public virtual void Delete(T t)
        {
            if (this.context.Entry(t).State == EntityState.Detached)
                this.dbSet.Attach(t);

            this.dbSet.Remove(t);
        }

        /// <summary>
        /// Updates only the root object - even if there are child objects attached.
        /// </summary>
        /// <param name="t"></param>
        public virtual void Update(T t)
        {
            this.dbSet.Attach(t);
            this.context.Entry(t).State = EntityState.Modified;
        }


        /// <summary>
        /// Updates the entire graph.  BaseData.EntityState on the root entity must be set to Modified.
        /// BaseData.EntityState on each object remaining determines the action Insert/Modify/Delete
        /// </summary>
        /// <param name="t"></param>
        public virtual void UpdateGraph(T t)
        {
            //List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Data.BaseData>> before = context.ChangeTracker.Entries<Data.BaseData>().ToList();
            this.dbSet.Add(t);
            this.context.ApplyStateChanges();
        }
        #endregion Methods
    }
}
