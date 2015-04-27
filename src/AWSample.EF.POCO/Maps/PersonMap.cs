using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.POCO.Maps
{
    public class PersonMap : EntityTypeConfiguration<AWSample.EF.POCO.Person.Person>
    {
        public PersonMap()
        {
            this.Property(e => e.PersonType)
                .IsFixedLength();

            this.HasMany(e => e.BusinessEntityContacts)
                .WithRequired(e => e.Person)
                .HasForeignKey(e => e.PersonID)
                .WillCascadeOnDelete(false);
        }
    }
}
