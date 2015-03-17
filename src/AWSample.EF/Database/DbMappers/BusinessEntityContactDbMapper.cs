using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.Database.Repositories;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.Database.DbMappers
{
    internal class BusinessEntityContactDbMapper
    {
        public BusinessEntityContactDbMapper(IBusinessEntityContactRepository unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region Variables
        private IBusinessEntityContactRepository unitOfWork;
        #endregion Variables

        #region Methods
        public IList<BusinessEntityContact> Get(Expression<Func<BusinessEntityContact, bool>> filter = null, Func<IQueryable<BusinessEntityContact>, IQueryable<BusinessEntityContact>> orderBy = null, ICollection<Expression<Func<BusinessEntityContact, object>>> includeProperties = null)
        {
            return this.unitOfWork.BusinessEntityContactRepository.Get(filter, orderBy, includeProperties).ToList();
        }

        public void Save(ICollection<BusinessEntityContact> entities)
        {
            if (entities == null)
                return;

            const string PIPE = "|";
            List<string> queryItems = entities.ToList().ConvertAll<string>(item => string.Concat(item.BusinessEntityID.ToString(), PIPE, item.PersonID.ToString(), PIPE, item.ContactTypeID.ToString()));
            Dictionary<string, BusinessEntityContact> existingEntities = unitOfWork.BusinessEntityContactRepository.Get(bec => queryItems.Contains(string.Concat(bec.BusinessEntityID.ToString(), PIPE, bec.PersonID.ToString(), PIPE, bec.ContactTypeID.ToString())))
                .ToDictionary(item => string.Concat(item.BusinessEntityID.ToString(), PIPE, item.PersonID.ToString(), PIPE, item.ContactTypeID.ToString()));

            foreach (BusinessEntityContact bec in entities)
            {
                string key = string.Concat(bec.BusinessEntityID.ToString(), PIPE, bec.PersonID.ToString(), PIPE, bec.ContactTypeID.ToString());
                switch (bec.EntityState)
                {
                    case AWSample.EF.POCO.EntityStateType.Deleted:
                        if (existingEntities.ContainsKey(key))
                            this.unitOfWork.BusinessEntityContactRepository.Delete(existingEntities[key]);
                        break;
                    case AWSample.EF.POCO.EntityStateType.Added:
                        this.unitOfWork.BusinessEntityContactRepository.Insert(bec);
                        break;
                    default:
                        if (!existingEntities.ContainsKey(key))
                        {
                            this.unitOfWork.BusinessEntityContactRepository.Insert(bec);
                        }
                        else
                        {
                            this.unitOfWork.Context.Entry(existingEntities[key]).CurrentValues.SetValues(bec);
                            this.unitOfWork.BusinessEntityContactRepository.Update(existingEntities[key]);
                        }
                        break;
                }
            }
        }

        public void Delete(ICollection<BusinessEntityContact> entities)
        {
            if (entities == null)
                return;

            const string PIPE = "|";
            List<string> queryItems = entities.ToList().ConvertAll<string>(item => string.Concat(item.BusinessEntityID.ToString(), PIPE, item.PersonID.ToString(), PIPE, item.ContactTypeID.ToString()));
            Dictionary<string, BusinessEntityContact> existingEntities = unitOfWork.BusinessEntityContactRepository.Get(bec => queryItems.Contains(string.Concat(bec.BusinessEntityID.ToString(), PIPE, bec.PersonID.ToString(), PIPE, bec.ContactTypeID.ToString())))
                .ToDictionary(item => string.Concat(item.BusinessEntityID.ToString(), PIPE, item.PersonID.ToString(), PIPE, item.ContactTypeID.ToString()));

            foreach (string key in existingEntities.Keys)
            {
                this.unitOfWork.BusinessEntityContactRepository.Delete(existingEntities[key]);
            }
        }
        #endregion Methods
    }
}
