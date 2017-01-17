using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Repositories.School;
using ContosoUniversity.Domain.School;
using ContosoUniversity.Utils;
using ContosoUniversity.Utils.Structures;
using Microsoft.EntityFrameworkCore.Query;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoUniversity.Controllers
{
    [Route("api/[controller]")]
    public class StudentsController : Controller
    {
        private IStudentRepository _studentRepository;

        public StudentsController(IStudentRepository studentRepository)
        {
            this._studentRepository = studentRepository;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await this._studentRepository.GetItemsAsync(null,
                                                        lst => lst.OrderBy(s => s.FullName)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Ordered/{searchString?}")]
        public async Task<IActionResult> Ordered(string searchString, [FromBody] SortCollection sorts)
        {
            try
            {
                Expression<Func<IQueryable<StudentModel>, IQueryable<StudentModel>>> queryExp = sorts.BuildOrderByExpression<StudentModel>();

                Task<int> countTask = this._studentRepository.CountAsync(string.IsNullOrEmpty(searchString) ? (Expression<Func<StudentModel, bool>>)null : n => n.FullName.Contains(searchString));

                Task<ICollection<StudentModel>> listTask = this._studentRepository.GetItemsAsync(
                    string.IsNullOrEmpty(searchString) ? (Expression<Func<StudentModel, bool>>)null : n => n.FullName.Contains(searchString),
                    queryExp);

                await Task.WhenAll(countTask, listTask);

                return Ok(new
                {
                    Students = listTask.Result,
                    StudentCount = countTask.Result
                });
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
                                await this._studentRepository.GetItemsAsync(d => d.ID == id,
                                                        lst => lst.OrderBy(s => s.FullName),
                                                        new Expression<Func<IQueryable<StudentModel>, IIncludableQueryable<StudentModel, object>>>[] { a => a.Include(x => x.Enrollments).ThenInclude(e => e.CourseTitle) })
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
        public async Task<IActionResult> Post([FromBody]StudentModel student)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                student.EntityState = Domain.EntityStateType.Added;
                if (await this._studentRepository.SaveAsync(student))
                {
                    return Created($"/api/[controller]/{student.ID}", student);
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
        public async Task<IActionResult> Put(int id, [FromBody]StudentModel student)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                student.EntityState = Domain.EntityStateType.Modified;
                if (await this._studentRepository.SaveAsync(student))
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
                if (await this._studentRepository.DeleteAsync(d => d.ID == id))
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
