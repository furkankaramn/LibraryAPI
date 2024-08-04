using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employees = await _context.Employees
                                          .Include(m => m.ApplicationUser)
                                          .ToListAsync();

            return Ok(employees);
        }

        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(string id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees
                                          .Include(m => m.ApplicationUser)
                                          .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(string id, Employee employee, string? currentPassword = null)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.Address = employee.ApplicationUser!.Address;
            applicationUser.BirthDate = employee.ApplicationUser!.BirthDate;
            applicationUser.Email = employee.ApplicationUser!.Email;
            // ...

            var updateResult = await _userManager.UpdateAsync(applicationUser);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            if (currentPassword != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(applicationUser, currentPassword, employee.ApplicationUser?.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(applicationUser);
            await _userManager.RemoveFromRolesAsync(applicationUser, currentRoles);

            string role = employee.Title.ToLower() switch
            {
                "admin" => "Admin",
                "worker" => "Worker",
                _ => "Worker"
            };

            var roleResult = await _userManager.AddToRoleAsync(applicationUser, role);
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            employee.ApplicationUser = null;
            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'ApplicationContext.Employees' is null.");
            }

            var result = await _userManager.CreateAsync(employee.ApplicationUser!, employee.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            string role = employee.Title.ToLower() switch
            {
                "admin" => "Admin",
                "worker" => "Worker",
                _ => "Worker"
            };

            var roleResult = await _userManager.AddToRoleAsync(employee.ApplicationUser, role);
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            employee.Id = employee.ApplicationUser!.Id;
            employee.ApplicationUser = null;
            _context.Employees.Add(employee);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmployeeExists(employee.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(string id)
        {
            return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
