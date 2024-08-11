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

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookCopiesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BookCopiesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/BookCopies
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookCopy>>> GetBookCopy()
        {
            return await _context.BookCopy.ToListAsync();
        }

        // GET: api/BookCopies/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookCopy>> GetBookCopy(int id)
        {
            var bookCopy = await _context.BookCopy.FindAsync(id);

            if (bookCopy == null)
            {
                return NotFound();
            }

            return bookCopy;
        }

        // PUT: api/BookCopies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookCopy(int id, BookCopy bookCopy)
        {
            if (id != bookCopy.Id)
            {
                return BadRequest();
            }

            _context.Entry(bookCopy).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookCopyExists(id))
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

        // POST: api/BookCopies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost]
        public async Task<ActionResult<BookCopy>> PostBookCopy(BookCopy bookCopy)
        {
            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookCopy.BookId);
            if (!bookExists)
            {
                return BadRequest("The book associated with this copy does not exist.");
            }

            _context.BookCopy.Add(bookCopy);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBookCopy", new { id = bookCopy.Id }, bookCopy);
        }

        // DELETE: api/BookCopies/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookCopy(int id)
        {
            var bookCopy = await _context.BookCopy.FindAsync(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            _context.BookCopy.Remove(bookCopy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookCopyExists(int id)
        {
            return _context.BookCopy.Any(e => e.Id == id);
        }
    }
}
