using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Domain.School
{
    public class EnrollmentModel : BaseModel
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
        [DisplayFormat(NullDisplayText = "No grade")]
        public Grade? Grade { get; set; }

        public string GradeLetter { get; set; }

        public string CourseTitle { get; set; }

        //public CourseModel Course { get; set; }
        //public StudentModel Student { get; set; }
    }

    public enum Grade
    {
        A, B, C, D, F
    }
}
