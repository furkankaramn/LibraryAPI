using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmployeesController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // GET: api/Employees
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees
                                        .Include(m => m.ApplicationUser)
                                        .ToListAsync();

            return Ok(employee);
        }

        // GET: api/Employees/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Worker")]
        [HttpGet("Me")]
        public async Task<ActionResult<Employee>> GetEmployee()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (userName == null)
            {
                return Unauthorized("Kullanıcı adı talep bilgilerinde bulunamadı.");
            }

            var employee = await _context.Employees
                                       .Include(m => m.ApplicationUser)
                                       .FirstOrDefaultAsync(m => m.ApplicationUser.UserName == userName);

            if (employee == null)
            {
                return NotFound($"Kullanıcı adı {userName} olan üye bulunamadı.");
            }

            return Ok(employee);
        }

        // PUT: api/Employees/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(string id, Employee employee, string? currentPassword = null)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı bilgilerini güncelle
            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser!.Address))
            {
                user.Address = employee.ApplicationUser.Address;
            }

            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser.FamilyName))
            {
                user.FamilyName = employee.ApplicationUser.FamilyName;
            }

            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser.Name))
            {
                user.Name = employee.ApplicationUser.Name;
            }

            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser.UserName))
            {
                user.UserName = employee.ApplicationUser.UserName;
            }

            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser.MiddleName))
            {
                user.MiddleName = employee.ApplicationUser.MiddleName;
            }

            if (!string.IsNullOrWhiteSpace(employee.ApplicationUser.Email))
            {
                user.Email = employee.ApplicationUser.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            if (currentPassword != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, employee.ApplicationUser.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            var employeeFromDb = await _context.Employees
                                               .Include(e => e.ApplicationUser)
                                               .FirstOrDefaultAsync(e => e.Id == user.Id);

            if (employeeFromDb == null)
            {
                return NotFound();
            }

            // Employee spesifik alanları güncelle
            if (!string.IsNullOrWhiteSpace(employee.Title))
            {
                employeeFromDb.Title = employee.Title;
            }

            if (!string.IsNullOrWhiteSpace(employee.Department))
            {
                employeeFromDb.Department = employee.Department;
            }

            _context.Entry(employeeFromDb).State = EntityState.Modified;
            _context.Entry(employeeFromDb).Property(e => e.Id).IsModified = false;

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

        // POST: api/Employees
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'ApplicationContext.Employees' is null.");
            }

            // Yeni kullanıcı oluşturma
            var result = await _userManager.CreateAsync(employee.ApplicationUser!, employee.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Role atama
            var roleResult = await _userManager.AddToRoleAsync(employee.ApplicationUser, "Worker");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            // Employee kaydını veritabanına ekleme
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

        // DELETE: api/Employees/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _userManager.FindByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(string id)
        {
            return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
