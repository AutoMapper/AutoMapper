using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.EF.Database.Repositories
{
    interface IDbContext
    {
        DbContext Context { get; }
    }
}
