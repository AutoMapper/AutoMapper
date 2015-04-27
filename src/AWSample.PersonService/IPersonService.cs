using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWSample.Domain.Person;

namespace AWSample.PersonService
{
    public interface IPersonService
    {
        ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null);
        int Count(Expression<Func<PersonModel, bool>> filter = null);
        void UpdateEmailPromotionAndPersonType(PersonModel entity);
        void UpdateBusinessContacts(PersonModel entity);
    }
}
