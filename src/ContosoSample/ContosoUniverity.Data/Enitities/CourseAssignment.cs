using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Data.Enitities
{
    [Table("CourseAssignment")]
    public class CourseAssignment : BaseData
    {
        public int InstructorID { get; set; }
        public int CourseID { get; set; }
        public Instructor Instructor { get; set; }
        public Course Course { get; set; }
    }
}
