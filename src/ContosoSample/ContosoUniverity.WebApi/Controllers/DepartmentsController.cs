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
    public class DepartmentsController : Controller
    {
        private IDepartmentRepository _departmentRepository;

        public DepartmentsController(IDepartmentRepository departmentRepository)
        {
            this._departmentRepository = departmentRepository;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await this._departmentRepository.GetItemsAsync(null,
                                                        lst => lst.OrderBy(s => s.Name),
                                                        new Expression<Func<IQueryable<DepartmentModel>, IIncludableQueryable<DepartmentModel, object>>>[] { i => i.Include(s => s.AdministratorName) }));
            }
            catch(Exception ex)
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
                                await this._departmentRepository.GetItemsAsync(d => d.DepartmentID == id,
                                                        lst => lst.OrderBy(s => s.Name),
                                                        new Expression<Func<IQueryable<DepartmentModel>, IIncludableQueryable<DepartmentModel, object>>>[] { i => i.Include(s => s.AdministratorName) })
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
        public async Task<IActionResult> Post([FromBody]DepartmentModel department)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                department.EntityState = Domain.EntityStateType.Added;
                if (await this._departmentRepository.SaveAsync(department))
                {
                    return Created($"/api/[controller]/{department.DepartmentID}", department);
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
        public async Task<IActionResult> Put(int id, [FromBody]DepartmentModel department)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not Saved");

            try
            {
                department.EntityState = Domain.EntityStateType.Modified;
                if (await this._departmentRepository.SaveAsync(department))
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
                if (await this._departmentRepository.DeleteAsync(d => d.DepartmentID == id))
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
