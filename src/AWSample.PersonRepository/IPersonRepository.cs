using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWSample.Domain.Person;

namespace AWSample.PersonRepository
{
    public interface IPersonRepository
    {
        ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null);
        int Count(Expression<Func<PersonModel, bool>> filter = null);
        void Save(PersonModel entity);
        void SaveGraph(PersonModel entity);
    }
}
