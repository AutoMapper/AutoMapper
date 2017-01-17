using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Repositories.School;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoUniversity.Controllers
{
    [Route("api/[controller]")]
    public class AboutController : Controller
    {
        IStudentRepository _schoolRepository;
        public AboutController(IStudentRepository schoolRepository)
        {
            this._schoolRepository = schoolRepository;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(
                    (
                    await this._schoolRepository.GetItemsAsync(null, lst => lst.OrderBy(s => s.EnrollmentDate)))
                    .GroupBy(s => s.EnrollmentDate)
                    .Select(grp => new { EnrollmentDate = grp.Key, StudentCount = grp.Count() }
                    )
                );
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
