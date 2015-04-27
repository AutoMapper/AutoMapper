using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.Database.Repositories;

namespace AWSample.EF.Person
{
    interface IPersonUnitOfWork : IDisposable, IPersonRepository, IBusinessEntityContactRepository, IDbContext
    {
        void Save();
    }
}
