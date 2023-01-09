using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiseaseService.Data;
using DiseaseService.Models;

namespace DiseaseService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CausesController : ControllerBase
    {
        private readonly DataContext _context;

        public CausesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Causes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cause>>> GetCauses()
        {
            return await _context.Causes.ToListAsync();
        }

        // GET: api/Causes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cause>> GetCause(long id)
        {
            var cause = await _context.Causes.FindAsync(id);

            if (cause == null)
            {
                return NotFound();
            }

            return cause;
        }

        // PUT: api/Causes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCause(long id, Cause cause)
        {
            if (id != cause.Id)
            {
                return BadRequest();
            }

            _context.Entry(cause).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CauseExists(id))
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

        // POST: api/Causes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cause>> PostCause(Cause cause)
        {
            _context.Causes.Add(cause);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCause", new { id = cause.Id }, cause);
        }

        // DELETE: api/Causes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCause(long id)
        {
            var cause = await _context.Causes.FindAsync(id);
            if (cause == null)
            {
                return NotFound();
            }

            cause.Status = false;

            _context.Entry(cause).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CauseExists(id))
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

        private bool CauseExists(long id)
        {
            return _context.Causes.Any(e => e.Id == id);
        }
    }
}
