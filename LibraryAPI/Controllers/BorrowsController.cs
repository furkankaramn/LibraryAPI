using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BorrowsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Borrows
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetBorrow()
        {
            return await _context.Borrow.ToListAsync();
        }

        // GET: api/Borrows/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Borrow>> GetBorrow(long id)
        {
            var borrow = await _context.Borrow.FindAsync(id);

            if (borrow == null)
            {
                return NotFound();
            }

            return borrow;
        }

        // PUT: api/Borrows/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBorrow(long id, Borrow borrow)
        {
            if (id != borrow.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve the existing Borrow entry
            var existingBorrow = await _context.Borrow
                .Include(b => b.Member)
                .Include(b => b.BookCopy)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBorrow == null)
            {
                return NotFound();
            }

            // Retrieve the related Member and BookCopy entities
            var member = await _context.Members.FindAsync(borrow.MembersId);
            var bookCopy = await _context.BookCopy.FindAsync(borrow.BookCopiesId);

            if (member == null)
            {
                return BadRequest("Member not found");
            }

            if (bookCopy == null)
            {
                return BadRequest("BookCopy not found");
            }

            // Calculate the delay and penalty fee
            if (borrow.TakenBackDate.HasValue && borrow.TakenBackDate > borrow.DeliveryDate)
            {
                TimeSpan delaySpan = borrow.TakenBackDate.Value - borrow.DeliveryDate;
                borrow.DelayTime = delaySpan.Days;
                borrow.PenaltyFee = borrow.DelayTime * 50;
            }
            else
            {
                borrow.DelayTime = 0;
                borrow.PenaltyFee = 0;
            }

            // Update member's total penalty fee
            if (existingBorrow.PenaltyFee.HasValue)
            {
                member.TotalPenaltyFee -= existingBorrow.PenaltyFee.Value;
            }
            if (borrow.PenaltyFee.HasValue)
            {
                member.TotalPenaltyFee += borrow.PenaltyFee.Value;
            }

            // Update the existing borrow entry
            existingBorrow.BookCopiesId = borrow.BookCopiesId;
            existingBorrow.EmployeesId = borrow.EmployeesId;
            existingBorrow.MembersId = borrow.MembersId;
            existingBorrow.PickUpDate = borrow.PickUpDate;
            existingBorrow.DeliveryDate = borrow.DeliveryDate;
            existingBorrow.TakenBackDate = borrow.TakenBackDate;
            existingBorrow.DelayTime = borrow.DelayTime;
            existingBorrow.PenaltyFee = borrow.PenaltyFee;

            // Update book copy status
            bookCopy.IsAvailable = !borrow.TakenBackDate.HasValue;

            // Mark entities as modified
            _context.Entry(existingBorrow).State = EntityState.Modified;
            _context.Entry(member).State = EntityState.Modified;
            _context.Entry(bookCopy).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowExists(id))
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




        // POST: api/Borrows
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost]
        public async Task<ActionResult<Borrow>> PostBorrow(Borrow borrow)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var employee = await _context.Employees
                                         .FirstOrDefaultAsync(e => e.ApplicationUser.UserName == userName);

            if (employee == null)
            {
                return Unauthorized("Employee not found.");
            }

            borrow.EmployeesId = employee.Id;

            var member = await _context.Members.FindAsync(borrow.MembersId);
            if (member == null)
            {
                return BadRequest("Member not found.");
            }

            var bookCopy = await _context.BookCopy.FindAsync(borrow.BookCopiesId);
            if (bookCopy == null)
            {
                return BadRequest("Invalid BookCopiesId.");
            }

            if (!bookCopy.IsAvailable)
            {
                return BadRequest("BookCopy is not available for borrowing.");
            }

            if (borrow.PenaltyFee.HasValue)
            {
                member.TotalPenaltyFee += borrow.PenaltyFee.Value;
            }

            _context.Borrow.Add(borrow);
            bookCopy.IsAvailable = false;
            _context.Entry(bookCopy).State = EntityState.Modified;
            _context.Entry(member).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrow", new { id = borrow.Id }, borrow);
        }


        // DELETE: api/Borrows/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrow(long id)
        {
            var borrow = await _context.Borrow.FindAsync(id);
            if (borrow == null)
            {
                return NotFound();
            }

            _context.Borrow.Remove(borrow);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BorrowExists(long id)
        {
            return _context.Borrow.Any(e => e.Id == id);
        }
    }
}
