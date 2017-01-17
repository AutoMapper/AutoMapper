using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data.Enitities;

namespace ContosoUniversity.Contexts.Maps
{
    public class CourseAssignmentMap : ITableMap
    {
        public void Map(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
        }
    }
}
