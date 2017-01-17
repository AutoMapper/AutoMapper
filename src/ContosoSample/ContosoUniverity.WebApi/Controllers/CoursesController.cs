using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ContosoUniversity.Repositories.School;
using ContosoUniversity.Domain.School;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoUniversity.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository)
        {
            this._courseRepository = courseRepository;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await this._courseRepository.GetItemsAsync(null,
                                                        lst => lst.OrderBy(s => s.Title),
                                                        new Expression<Func<IQueryable<CourseModel>, IIncludableQueryable<CourseModel, object>>>[] { i => i.Include(s => s.DepartmentName) }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(
                            (
                                await this._courseRepository.GetItemsAsync(d => d.CourseID == id,
                                                        lst => lst.OrderBy(s => s.Title),
                                                        new Expression<Func<IQueryable<CourseModel>, IIncludableQueryable<CourseModel, object>>>[] { i => i.Include(s => s.DepartmentName) })
                            ).Single()

                         );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CourseModel course)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                CourseModel crs = (await this._courseRepository.GetItemsAsync(d => d.CourseID == course.CourseID)).SingleOrDefault();
                if (crs != null)
                    return BadRequest("Couse ID Exists");

                course.EntityState = Domain.EntityStateType.Added;
                if (await this._courseRepository.SaveAsync(course))
                {
                    return Created($"/api/[controller]/{course.CourseID}", course);
                }
                else
                {
                    return BadRequest("Not Saved");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]CourseModel course)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                course.EntityState = Domain.EntityStateType.Modified;
                if (await this._courseRepository.SaveAsync(course))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Not Saved");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (await this._courseRepository.DeleteAsync(d => d.CourseID == id))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Not Saved");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
