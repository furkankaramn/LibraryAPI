using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Member")]
        [HttpGet("Me")]
        public async Task<ActionResult<Member>> GetMember()
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

        [Authorize(Roles = "Member")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(string id, Member member, string? currentPassword = null)
        {
            if (id != member.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Update user details
            user.Address = member.ApplicationUser?.Address;
            user.Email = member.ApplicationUser?.Email;
            user.FamilyName = member.ApplicationUser?.FamilyName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            if (currentPassword != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, member.ApplicationUser?.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            var memberFromDb = await _context.Members
                                             .Include(m => m.ApplicationUser)
                                             .FirstOrDefaultAsync(m => m.Id == user.Id);

            if (memberFromDb == null)
            {
                return NotFound();
            }

            memberFromDb.EducationalDegree = member.EducationalDegree;

            _context.Entry(memberFromDb).State = EntityState.Modified;
            _context.Entry(memberFromDb).Property(m => m.Id).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'ApplicationContext.Members' is null.");
            }

            var result = await _userManager.CreateAsync(member.ApplicationUser!, member.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(member.ApplicationUser, "Member");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(string id)
        {
            return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
