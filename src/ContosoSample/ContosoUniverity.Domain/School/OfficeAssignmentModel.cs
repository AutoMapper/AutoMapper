using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Domain.School
{
    public class OfficeAssignmentModel : BaseModel
    {
        public int InstructorID { get; set; }
        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        //public virtual InstructorModel Instructor { get; set; }
    }
}
