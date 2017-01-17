using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;

namespace ContosoUniversity.Crud.DbMappers
{
    internal class DbMapperBase<T> : IDbMapper<T> where T : BaseData
    {
        public DbMapperBase(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region Variables
        private IUnitOfWork unitOfWork;
        #endregion Variables

        #region Properties
        protected IUnitOfWork UnitOfWork { get { return this.unitOfWork; } }
        protected virtual GenericRepository<T> Repository { get { return this.unitOfWork.GetRepository<T>(); } }
        #endregion Properties

        #region Methods
        public virtual void AddChanges(ICollection<T> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            foreach (T entity in entities)
            {
                switch (entity.EntityState)
                {
                    case EntityStateType.Deleted:
                        this.Repository.Delete(entity);
                        break;
                    case EntityStateType.Modified:
                        //this.RemoveNavigationProperties(entity);
                        this.Repository.Update(entity);
                        break;
                    case EntityStateType.Added:
                        //this.RemoveNavigationProperties(entity);
                        this.Repository.Insert(entity);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void AddGraphChanges(ICollection<T> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            foreach (T entity in entities)
            {
                switch (entity.EntityState)
                {
                    case EntityStateType.Deleted:
                        this.Repository.Delete(entity);
                        break;
                    case EntityStateType.Added:
                        this.Repository.InsertGraph(entity);
                        break;
                    case EntityStateType.Modified:
                        this.Repository.UpdateGraph(entity);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// EF tries to add child objects to the context when they (the child objects) are not already in the database.
        /// RemoveNavigationProperties is a place to remove child objects when we call Save (to save just the entity) instead of SaveGraphs.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void RemoveNavigationProperties(T entity)
        {
            //System.Reflection.PropertyInfo[] infos = typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            /*System.Reflection.PropertyInfo[] infos = typeof(T).GetProperties();
            infos.ToList().ForEach(info =>
            {
                if (!(info.PropertyType == typeof(string)
                        || info.PropertyType.IsValueType
                        || (info.PropertyType.IsGenericType
                            && info.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                            && Nullable.GetUnderlyingType(info.PropertyType).IsValueType)))
                {
                    info.SetValue(entity, null);
                }
            });*/
        }
        #endregion Methods
    }
}
