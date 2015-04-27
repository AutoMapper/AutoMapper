using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.EF
{
    internal class GenericRepository<T> where T : class
    {
        private DbContext context;
        private DbSet<T> dbSet;

        public GenericRepository(DbContext context)
        {
            this.context = context;
            this.context.Configuration.LazyLoadingEnabled = false;
            this.context.Configuration.ProxyCreationEnabled = false;
            this.dbSet = context.Set<T>();
        }

        /// <summary>
        /// Expression<Func<T,bool>> filter e.g. student => student.Name == smith
        /// Func<IQueryable<T>, IQueryable<T>> orderBy e.g. q => q.OrderBy(s => s.name)
        /// ICollection<Expression<Func<T, object>>> includeProperties e.g. new ICollection<Expression<Func<T, object>>> { user => user.Address, user => user.Roles }
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IList<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IQueryable<T>> orderBy = null, ICollection<Expression<Func<T, object>>> includeProperties = null)
        {
            IQueryable<T> query = this.dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (includeProperties != null)
            {
                foreach (Expression<Func<T, object>> includeProperty in includeProperties)
                    query = query.Include(includeProperty);
            }

            if (orderBy != null)
                return orderBy(query).ToList();
            else
                return query.ToList();
        }

        /// <summary>
        /// Returns a count of all rows to be returned by the query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = this.dbSet;

            if (filter != null)
                query = query.Where(filter);

            return query.Count();
        }

        public virtual T GetById(object id)
        {
            return this.dbSet.Find(id);
        }

        public virtual void InsertGraph(T t)
        {
            this.dbSet.Add(t);
        }

        public virtual void Insert(T t)
        {
            this.context.Entry(t).State = EntityState.Added;//EF attached entity to context if not attached
        }

        public virtual void Delete(object id)
        {
            T t = this.dbSet.Find(id);
            Delete(t);
        }

        public virtual void Delete(T t)
        {
            if (this.context.Entry(t).State == EntityState.Detached)
                this.dbSet.Attach(t);

            this.dbSet.Remove(t);
        }

        public virtual void Update(T t)
        {
            if (this.context.Entry(t).State == EntityState.Detached)
                this.dbSet.Attach(t);

            this.context.Entry(t).State = EntityState.Modified;
        }
    }
}
