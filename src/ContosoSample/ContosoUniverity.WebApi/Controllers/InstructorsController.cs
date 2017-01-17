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
    public class InstructorsController : Controller
    {
        private IInstructorRepository _instructorRepository;

        public InstructorsController(IInstructorRepository instructorRepository)
        {
            this._instructorRepository = instructorRepository;
        }
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                //Use  return Created($"api/trips/{theTrip.Name}", Mapper.Map<TripViewModel>(newTrip)); for POST
                // new NoContentResult() or NoContent(); for Delete or Update
                return Ok(await this._instructorRepository.GetItemsAsync(null,
                                                        lst => lst.OrderBy(s => s.FullName)));
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
                                await this._instructorRepository.GetItemsAsync(d => d.ID == id,
                                                        lst => lst.OrderBy(s => s.FullName),
                                                        new Expression<Func<IQueryable<InstructorModel>, IIncludableQueryable<InstructorModel, object>>>[] { i => i.Include(s => s.Courses) })
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
        public async Task<IActionResult> Post([FromBody]InstructorModel instructor)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                instructor.EntityState = Domain.EntityStateType.Added;
                if (await this._instructorRepository.SaveAsync(instructor))
                {
                    return Created($"/api/[controller]/{instructor.ID}", instructor);
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
        public async Task<IActionResult> Put(int id, [FromBody]InstructorModel instructor)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                instructor.EntityState = Domain.EntityStateType.Modified;
                if (await this._instructorRepository.SaveAsync(instructor))
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
                if (await this._instructorRepository.DeleteAsync(d => d.ID == id))
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
