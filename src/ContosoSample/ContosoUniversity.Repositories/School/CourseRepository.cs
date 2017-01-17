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
    public interface ICourseRepository : IRepositoryBase<CourseModel, Course>
    {
    }
    public class CourseRepository : RepositoryBase<CourseModel, Course>, ICourseRepository
    {
        public CourseRepository(ISchoolStore store, IMapper mapper) : base(store, mapper)
        {
        }
    }
}
