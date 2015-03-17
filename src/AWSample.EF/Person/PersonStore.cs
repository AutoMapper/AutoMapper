using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.Database.DbMappers;

namespace AWSample.EF.Person
{
    public class PersonStore : IPersonStore
    {
        public IList<AWSample.EF.POCO.Person.Person> Get(Expression<Func<AWSample.EF.POCO.Person.Person, bool>> filter = null, Func<IQueryable<AWSample.EF.POCO.Person.Person>, IQueryable<AWSample.EF.POCO.Person.Person>> orderBy = null, ICollection<Expression<Func<AWSample.EF.POCO.Person.Person, object>>> includeProperties = null)
        {
            IList<AWSample.EF.POCO.Person.Person> list = null;
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                list = new PersonDbMapper(unitOfWork).Get(filter, orderBy, includeProperties);
            }

            return list;
        }

        public int Count(Expression<Func<AWSample.EF.POCO.Person.Person, bool>> filter = null)
        {
            int count = 0;
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                count = unitOfWork.PersonRepository.Count(filter);
            }

            return count;
        }

        public void Save(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                new PersonDbMapper(unitOfWork).Save(entities);
                unitOfWork.Save();
            }
        }

        public void SaveGraphs(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                new PersonDbMapper(unitOfWork).SaveGraphs(entities);
                unitOfWork.Save();
            }
        }

        public void Delete(ICollection<AWSample.EF.POCO.Person.Person> entities)
        {
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                new PersonDbMapper(unitOfWork).Delete(entities);
                unitOfWork.Save();
            }
        }
    }
}
