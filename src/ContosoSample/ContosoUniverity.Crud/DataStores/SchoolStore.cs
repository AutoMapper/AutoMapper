using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ContosoUniversity.Contexts;
using ContosoUniversity.Data;

namespace ContosoUniversity.Crud.DataStores
{
    public class SchoolStore : StoreBase, ISchoolStore
    {
        public SchoolStore(SchoolContext context)
            : base(new UnitOfWork(context))
        {
            _context = context;
        }

        #region Fields
        private readonly SchoolContext _context; 
        #endregion
    }
}
