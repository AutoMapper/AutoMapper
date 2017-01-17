using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Domain.School
{
    public class CourseModel : BaseModel
    {
        [Display(Name = "Number")]
        public int CourseID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Range(0, 5)]
        public int Credits { get; set; }

        public int DepartmentID { get; set; }

        public string DepartmentName { get; set; }

        //public DepartmentModel Department { get; set; }
        //public ICollection<EnrollmentModel> Enrollments { get; set; }
        public ICollection<CourseAssignmentModel> Assignments { get; set; }
    }
}
