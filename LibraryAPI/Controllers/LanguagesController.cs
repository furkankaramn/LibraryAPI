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
    public class LanguagesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public LanguagesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Languages
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Language>>> GetLanguages()
        {
            return await _context.Languages.ToListAsync();
        }

        // GET: api/Languages/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Language>> GetLanguage(string id)
        {
            var language = await _context.Languages.FindAsync(id);

            if (language == null)
            {
                return NotFound();
            }

            return language;
        }

        // PUT: api/Languages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanguage(string id, Language language)
        {
            if (id != language.Code)
            {
                return BadRequest();
            }

            _context.Entry(language).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LanguageExists(id))
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

        // POST: api/Languages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost]
        public async Task<ActionResult<Language>> PostLanguage(Language language)
        {
            _context.Languages.Add(language);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LanguageExists(language.Code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetLanguage", new { id = language.Code }, language);
        }

        // DELETE: api/Languages/5
        [Authorize(Roles = "Worker,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(string id)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return NotFound();
            }

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LanguageExists(string id)
        {
            return _context.Languages.Any(e => e.Code == id);
        }
    }
}
