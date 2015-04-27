using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.POCO.Maps;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.Contexts
{
    public class PersonContext : DbContext
    {
        public PersonContext()
            : base("AdventureWorks")
        {
            Database.SetInitializer<PersonContext>(null);
        }

        public DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; }
        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new BusinessEntityContactMap());
            modelBuilder.Configurations.Add(new PersonMap());
        }
    }
}
