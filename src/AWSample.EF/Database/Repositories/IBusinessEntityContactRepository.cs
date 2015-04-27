using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.Database.Repositories
{
    interface IBusinessEntityContactRepository : IDbContext
    {
        GenericRepository<BusinessEntityContact> BusinessEntityContactRepository { get; }
    }
}
