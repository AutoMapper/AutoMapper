using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AWSample.Domain.Person;
using AWSample.EF.Person;
using XpressionMapper.Extensions;

namespace AWSample.PersonRepository
{
    public class PersonRepository : IPersonRepository
    {
        public PersonRepository()
        {
            this.store = new PersonStore();
        }

        public PersonRepository(IPersonStore store)
        {
            this.store = store;
        }

        #region Variables
        private IPersonStore store;
        #endregion Variables

        #region Methods
        public ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null)
        {
            Expression<Func<AWSample.EF.POCO.Person.Person, bool>> f = filter.MapExpression<PersonModel, AWSample.EF.POCO.Person.Person, bool>();
            Expression<Func<IQueryable<AWSample.EF.POCO.Person.Person>, IQueryable<AWSample.EF.POCO.Person.Person>>> mappedOrderBy = orderBy.MapOrderByExpression<PersonModel, AWSample.EF.POCO.Person.Person>();
            ICollection<Expression<Func<AWSample.EF.POCO.Person.Person, object>>> includes = includeProperties.MapExpressionList<PersonModel, AWSample.EF.POCO.Person.Person, object>();

            ICollection<AWSample.EF.POCO.Person.Person> list = store.Get(f, mappedOrderBy == null ? null : mappedOrderBy.Compile(), includes);
            return Mapper.Map<IEnumerable<AWSample.EF.POCO.Person.Person>, IEnumerable<PersonModel>>(list).ToList();
        }

        public int Count(Expression<Func<PersonModel, bool>> filter = null)
        {
            Expression<Func<AWSample.EF.POCO.Person.Person, bool>> f = filter.MapExpression<PersonModel, AWSample.EF.POCO.Person.Person, bool>();
            return store.Count(f);
        }

        public void Save(PersonModel entity)
        {
            AWSample.EF.POCO.Person.Person item = Mapper.Map<AWSample.EF.POCO.Person.Person>(entity);
            store.Save(new List<AWSample.EF.POCO.Person.Person> { item });
        }

        public void SaveGraph(PersonModel entity)
        {
            AWSample.EF.POCO.Person.Person item = Mapper.Map<AWSample.EF.POCO.Person.Person>(entity);
            store.SaveGraphs(new List<AWSample.EF.POCO.Person.Person> { item });
        }
        #endregion Methods
    }
}
