using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.POCO.Maps
{
    public class BusinessEntityContactMap : EntityTypeConfiguration<BusinessEntityContact>
    {
        public BusinessEntityContactMap()
        {
        }
    }
}
