using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Domain.School
{
    public class CourseAssignmentModel : BaseModel
    {
        public int InstructorID { get; set; }
        public int CourseID { get; set; }
        //public InstructorModel Instructor { get; set; }
        //public CourseModel Course { get; set; }
    }
}
