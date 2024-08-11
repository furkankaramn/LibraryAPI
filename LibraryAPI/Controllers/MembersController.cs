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
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public MembersController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            if (_context.Members == null)
            {
                return NotFound();
            }


            var members = await _context.Members
                                        .Include(m => m.ApplicationUser)
                                        .ToListAsync();

            return Ok(members);
        }
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members
                                       .Include(m => m.ApplicationUser)
                                       .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // GET: api/Members
        [Authorize(Roles = "Member")]

        [HttpGet("Me")]
        public async Task<ActionResult<Member>> Getmember()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (userName == null)
            {
                return Unauthorized("Kullanıcı adı talep bilgilerinde bulunamadı.");
            }

            var member = await _context.Members
                                       .Include(m => m.ApplicationUser)
                                       .FirstOrDefaultAsync(m => m.ApplicationUser.UserName == userName);

            if (member == null)
            {
                return NotFound($"Kullanıcı adı {userName} olan üye bulunamadı.");
            }

            return Ok(member);
        }


        // PUT: api/Members
        [Authorize(Roles = "Member")]
        [HttpPut("Me")]
        public async Task<IActionResult> PutMember(Member member)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null)
            {
                return Unauthorized("Kullanıcı adı talep bilgilerinde bulunamadı.");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı bilgilerini güncelle
            if (!string.IsNullOrWhiteSpace(member.ApplicationUser!.Address))
            {
                user.Address = member.ApplicationUser.Address;
            }

            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.FamilyName))
            {
                user.FamilyName = member.ApplicationUser.FamilyName;
            }

            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.Name))
            {
                user.Name = member.ApplicationUser.Name; 
            }
            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.UserName))
            {
                user.UserName = member.ApplicationUser.UserName;
            }
            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.MiddleName))
            {
                user.MiddleName = member.ApplicationUser.MiddleName;
            }
            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.Name))
            {
                user.Name = member.ApplicationUser.Name;
            }
            if (!string.IsNullOrWhiteSpace(member.ApplicationUser.Email))
            {
                user.Email = member.ApplicationUser.Email;
            }
            

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            var memberFromDb = await _context.Members
                                             .Include(m => m.ApplicationUser)
                                             .FirstOrDefaultAsync(m => m.Id == user.Id);

            if (memberFromDb == null)
            {
                return NotFound();
            }

            
            if (member.EducationalDegree != null)
            {
                memberFromDb.EducationalDegree = member.EducationalDegree;
            }


            _context.Entry(memberFromDb).State = EntityState.Modified;
            _context.Entry(memberFromDb).Property(m => m.Id).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(user.Id))
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



        // POST: api/Members

        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'ApplicationContext.Members' is null.");
            }

            // Yeni kullanıcı oluşturma
            var result = await _userManager.CreateAsync(member.ApplicationUser!, member.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Role atama
            var roleResult = await _userManager.AddToRoleAsync(member.ApplicationUser, "Member");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            // Member kaydını veritabanına ekleme
            member.Id = member.ApplicationUser!.Id;
            member.ApplicationUser = null;
            _context.Members.Add(member);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MemberExists(member.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMember", new { id = member.Id }, member);
        }

        // DELETE: api/Members
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _userManager.FindByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            member.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        private bool MemberExists(string id)
        {
            return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}