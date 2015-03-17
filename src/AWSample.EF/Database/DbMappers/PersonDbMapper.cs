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
    internal class PersonDbMapper
    {
        public PersonDbMapper(IPersonRepository unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region Variables
        private IPersonRepository unitOfWork;
        #endregion Variables

        #region Methods
        public IList<AWSample.EF.POCO.Person.Person> Get(Expression<Func<AWSample.EF.POCO.Person.Person, bool>> filter = null, Func<IQueryable<AWSample.EF.POCO.Person.Person>, IQueryable<AWSample.EF.POCO.Person.Person>> orderBy = null, ICollection<Expression<Func<AWSample.EF.POCO.Person.Person, object>>> includeProperties = null)
        {
            return this.unitOfWork.PersonRepository.Get(filter, orderBy, includeProperties).ToList();
        }

        public void Save(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            if (entities == null)
                return;

            int[] ids = entities.Select(item => item.BusinessEntityID).ToArray();
            Dictionary<int, AWSample.EF.POCO.Person.Person> existingEntities = unitOfWork.PersonRepository.Get(person => ids.Contains(person.BusinessEntityID)).ToDictionary(item => item.BusinessEntityID);

            foreach (AWSample.EF.POCO.Person.Person person in entities)
            {
                switch (person.EntityState)
                {
                    case AWSample.EF.POCO.EntityStateType.Deleted:
                        if (existingEntities.ContainsKey(person.BusinessEntityID))
                            this.unitOfWork.PersonRepository.Delete(existingEntities[person.BusinessEntityID]);
                        break;
                    case AWSample.EF.POCO.EntityStateType.Added:
                        this.unitOfWork.PersonRepository.Insert(person);
                        break;
                    default:
                        if (!existingEntities.ContainsKey(person.BusinessEntityID))
                        {
                            this.unitOfWork.PersonRepository.Insert(person);
                        }
                        else
                        {
                            this.unitOfWork.Context.Entry(existingEntities[person.BusinessEntityID]).CurrentValues.SetValues(person);
                            unitOfWork.PersonRepository.Update(existingEntities[person.BusinessEntityID]);
                        }
                        break;
                }
            }
        }

        public void SaveGraphs(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            if (entities == null)
                return;

            int[] userIds = entities.Select(item => item.BusinessEntityID).ToArray();
            Dictionary<int, AWSample.EF.POCO.Person.Person> existingEntities = unitOfWork.PersonRepository.Get(user => userIds.Contains(user.BusinessEntityID)).ToDictionary(item => item.BusinessEntityID);

            foreach (AWSample.EF.POCO.Person.Person person in entities)
            {
                switch (person.EntityState)
                {
                    case AWSample.EF.POCO.EntityStateType.Deleted:
                        if (existingEntities.ContainsKey(person.BusinessEntityID))
                            this.unitOfWork.PersonRepository.Delete(existingEntities[person.BusinessEntityID]);
                        break;
                    case AWSample.EF.POCO.EntityStateType.Added:
                        this.unitOfWork.PersonRepository.InsertGraph(person);
                        break;
                    default:
                        if (!existingEntities.ContainsKey(person.BusinessEntityID))
                        {
                            this.unitOfWork.PersonRepository.InsertGraph(person);
                        }
                        else
                        {
                            this.unitOfWork.Context.Entry(existingEntities[person.BusinessEntityID]).CurrentValues.SetValues(person);
                            new BusinessEntityContactDbMapper(unitOfWork).Save(person.BusinessEntityContacts);
                            unitOfWork.PersonRepository.Update(existingEntities[person.BusinessEntityID]);
                        }
                        break;
                }
            }
        }

        public void Delete(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            if (entities == null)
                return;

            int[] ids = entities.Select(item => item.BusinessEntityID).ToArray();
            Dictionary<int, AWSample.EF.POCO.Person.Person> existingEntities = unitOfWork.PersonRepository.Get(user => ids.Contains(user.BusinessEntityID)).ToDictionary(item => item.BusinessEntityID);
            foreach (int key in existingEntities.Keys)
            {
                this.unitOfWork.PersonRepository.Delete(existingEntities[key]);
            }
        }

        public void Delete(ICollection<int> ids)
        {
            if (ids == null)
                return;

            Dictionary<int, AWSample.EF.POCO.Person.Person> existingEntities = unitOfWork.PersonRepository.Get(user => ids.Contains(user.BusinessEntityID)).ToDictionary(item => item.BusinessEntityID);
            foreach (int key in existingEntities.Keys)
            {
                this.unitOfWork.PersonRepository.Delete(existingEntities[key]);
            }
        }
        #endregion Methods
    }
}
