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
    public class BookSubCategoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BookSubCategoriesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/BookSubCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookSubCategory>>> GetBookSubCategory()
        {
            return await _context.BookSubCategory.ToListAsync();
        }

        // GET: api/BookSubCategories/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookSubCategory>> GetBookSubCategory(short id)
        {
            var bookSubCategory = await _context.BookSubCategory.FindAsync(id);

            if (bookSubCategory == null)
            {
                return NotFound();
            }

            return bookSubCategory;
        }

        // PUT: api/BookSubCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookSubCategory(short id, BookSubCategory bookSubCategory)
        {
            if (id != bookSubCategory.SubCategoriesId)
            {
                return BadRequest();
            }

            _context.Entry(bookSubCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookSubCategoryExists(id))
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

        // POST: api/BookSubCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost]
        public async Task<ActionResult<BookSubCategory>> PostBookSubCategory(BookSubCategory bookSubCategory)
        {
            _context.BookSubCategory.Add(bookSubCategory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BookSubCategoryExists(bookSubCategory.SubCategoriesId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBookSubCategory", new { id = bookSubCategory.SubCategoriesId }, bookSubCategory);
        }

        // DELETE: api/BookSubCategories/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookSubCategory(short id)
        {
            var bookSubCategory = await _context.BookSubCategory.FindAsync(id);
            if (bookSubCategory == null)
            {
                return NotFound();
            }

            _context.BookSubCategory.Remove(bookSubCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookSubCategoryExists(short id)
        {
            return _context.BookSubCategory.Any(e => e.SubCategoriesId == id);
        }
    }
}
