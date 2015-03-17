using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.EF.Person
{
    public interface IPersonStore
    {
        void Delete(ICollection<AWSample.EF.POCO.Person.Person> entities);
        IList<AWSample.EF.POCO.Person.Person> Get(Expression<Func<AWSample.EF.POCO.Person.Person, bool>> filter = null, Func<IQueryable<AWSample.EF.POCO.Person.Person>, IQueryable<AWSample.EF.POCO.Person.Person>> orderBy = null, ICollection<Expression<Func<AWSample.EF.POCO.Person.Person, object>>> includeProperties = null);
        int Count(Expression<Func<AWSample.EF.POCO.Person.Person, bool>> filter = null);
        void Save(ICollection<AWSample.EF.POCO.Person.Person> entities);
        void SaveGraphs(ICollection<AWSample.EF.POCO.Person.Person> entities);
    }
}
