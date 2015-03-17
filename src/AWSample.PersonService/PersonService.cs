using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWSample.Domain.Person;
using AWSample.PersonRepository;

namespace AWSample.PersonService
{
    public class PersonService : IPersonService
    {
        public PersonService()
        {
            this.repository = new AWSample.PersonRepository.PersonRepository();
        }

        public PersonService(IPersonRepository repository)
        {
            this.repository = repository;
        }

        #region Variables
        private IPersonRepository repository;
        #endregion Variables

        public ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null)
        {
            ICollection<PersonModel> changes = repository.GetList(filter, orderBy, includeProperties);
            return changes.ToList();
        }

        public int Count(System.Linq.Expressions.Expression<Func<Domain.Person.PersonModel, bool>> filter = null)
        {
            return repository.Count(filter);
        }

        public void UpdateEmailPromotionAndPersonType(PersonModel entity)
        {
            PersonModel fromDataBase = repository.GetList(item => item.BusinessEntityID == entity.BusinessEntityID).SingleOrDefault();
            fromDataBase.EmailPromotion = entity.EmailPromotion;
            fromDataBase.PersonType = entity.PersonType;

            repository.Save(fromDataBase);
            //If you're sure entity includes all required fields simply call repository.Save(entity);
        }

        public void UpdateBusinessContacts(PersonModel entity)
        {
            PersonModel fromDataBase = repository.GetList(item => item.BusinessEntityID == entity.BusinessEntityID, null, new List<Expression<Func<PersonModel, object>>>() { item => item.BusinessEntityContacts }).SingleOrDefault();

            foreach (BusinessEntityContactModel bcmFromDb in fromDataBase.BusinessEntityContacts)
            {
                BusinessEntityContactModel bcmEntity = entity.BusinessEntityContacts.Where(item => item.BusinessEntityID == bcmFromDb.BusinessEntityID
                    && item.ContactTypeID == bcmFromDb.ContactTypeID
                    && item.PersonID == bcmFromDb.PersonID).SingleOrDefault();

                if (bcmEntity != null)
                    bcmFromDb.ModifiedDate = bcmEntity.ModifiedDate;
            }

            repository.SaveGraph(fromDataBase);
            //If you're sure entity and its BusinessEntityContacts includes all required fields simply call repository.SaveGraph(entity);
        }
    }
}
