using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.Contexts;
using AWSample.EF.Database.Repositories;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.Person
{
    internal class PersonUnitOfWork : UnitOfWorkBase, IPersonUnitOfWork
    {
        #region Variables
        private DbContext context = new PersonContext();
        private GenericRepository<BusinessEntityContact> businessEntityContactRepository;
        private GenericRepository<AWSample.EF.POCO.Person.Person> personRepository;
        #endregion Variables

        #region Properties
        public override DbContext Context { get { return context; } }

        public GenericRepository<BusinessEntityContact> BusinessEntityContactRepository
        {
            get
            {
                if (this.businessEntityContactRepository == null)
                    this.businessEntityContactRepository = new GenericRepository<BusinessEntityContact>(this.context);

                return this.businessEntityContactRepository;
            }
        }

        public GenericRepository<AWSample.EF.POCO.Person.Person> PersonRepository
        {
            get
            {
                if (this.personRepository == null)
                    this.personRepository = new GenericRepository<AWSample.EF.POCO.Person.Person>(this.context);

                return this.personRepository;
            }
        }
        #endregion Properties
    }
}
