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
    public class BookLanguagesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BookLanguagesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/BookLanguages
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookLanguage>>> GetBookLanguage()
        {
            return await _context.BookLanguage.ToListAsync();
        }

        // GET: api/BookLanguages/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookLanguage>> GetBookLanguage(string id)
        {
            var bookLanguage = await _context.BookLanguage.FindAsync(id);

            if (bookLanguage == null)
            {
                return NotFound();
            }

            return bookLanguage;
        }

        // PUT: api/BookLanguages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookLanguage(string id, BookLanguage bookLanguage)
        {
            if (id != bookLanguage.LanguagesCode)
            {
                return BadRequest();
            }

            _context.Entry(bookLanguage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookLanguageExists(id))
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

        // POST: api/BookLanguages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost]
        public async Task<ActionResult<BookLanguage>> PostBookLanguage(BookLanguage bookLanguage)
        {
            _context.BookLanguage.Add(bookLanguage);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BookLanguageExists(bookLanguage.LanguagesCode))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBookLanguage", new { id = bookLanguage.LanguagesCode }, bookLanguage);
        }

        // DELETE: api/BookLanguages/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookLanguage(string id)
        {
            var bookLanguage = await _context.BookLanguage.FindAsync(id);
            if (bookLanguage == null)
            {
                return NotFound();
            }

            _context.BookLanguage.Remove(bookLanguage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookLanguageExists(string id)
        {
            return _context.BookLanguage.Any(e => e.LanguagesCode == id);
        }
    }
}
