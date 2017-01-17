using ContosoUniversity.Data.Enitities;
using ContosoUniversity.Domain.School;

namespace ContosoUniversity.Repositories.School
{
    public interface IStudentRepository : IRepositoryBase<StudentModel, Student>
    {
    }
}
