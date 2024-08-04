using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RezervationsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public RezervationsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Rezervations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rezervation>>> GetRezervation()
        {
            return await _context.Rezervation.ToListAsync();
        }

        // GET: api/Rezervations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rezervation>> GetRezervation(long id)
        {
            var rezervation = await _context.Rezervation.FindAsync(id);

            if (rezervation == null)
            {
                return NotFound();
            }

            return rezervation;
        }

        // PUT: api/Rezervations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRezervation(long id, Rezervation rezervation)
        {
            if (id != rezervation.Id)
            {
                return BadRequest();
            }

            _context.Entry(rezervation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RezervationExists(id))
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

        // POST: api/Rezervations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Rezervation>> PostRezervation(Rezervation rezervation)
        {
            _context.Rezervation.Add(rezervation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRezervation", new { id = rezervation.Id }, rezervation);
        }

        // DELETE: api/Rezervations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRezervation(long id)
        {
            var rezervation = await _context.Rezervation.FindAsync(id);
            if (rezervation == null)
            {
                return NotFound();
            }

            _context.Rezervation.Remove(rezervation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RezervationExists(long id)
        {
            return _context.Rezervation.Any(e => e.Id == id);
        }
    }
}
