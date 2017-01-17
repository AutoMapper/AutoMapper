using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ContosoUniversity.Crud.DataStores;
using ContosoUniversity.Data.Enitities;
using ContosoUniversity.Domain.School;

namespace ContosoUniversity.Repositories.School
{
    public interface IDepartmentRepository : IRepositoryBase<DepartmentModel, Department>
    {
    }
    public class DepartmentRepository : RepositoryBase<DepartmentModel, Department>, IDepartmentRepository
    {
        public DepartmentRepository(ISchoolStore store, IMapper mapper) : base(store, mapper)
        {
        }
    }
}
