﻿using System;
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
            var cause = await _context.Causes.Include(cs => cs.CauseImages).FirstOrDefaultAsync(cs => cs.Id == id);

            if (cause == null)
            {
                return NotFound();
            }

            return cause;
        }

        [HttpGet("GetImages/{id}")]
        public async Task<ActionResult<List<CauseImages>>> GetImages(long id)
        {
            var images = await _context.CauseImages.Where(cs => cs.CauseId == id).ToListAsync();

            if (images == null)
            {
                return NotFound();
            }

            return images;
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

        [HttpPost("PostCauseImages")]
        public async Task<ActionResult<CauseImages>> PostCauseImages(CauseImages causeImages)
        {
            _context.CauseImages.Add(causeImages);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCause", new { id = causeImages.Id }, causeImages);
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

        [HttpDelete("DeleteImages/{id}")]
        public async Task<IActionResult> DeleteImages(int id)
        {
            var images = await _context.CauseImages.FindAsync(id);
            if (images == null)
            {
                return NotFound();
            }

            images.Status = false;

            _context.Entry(images).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImagesExists(id))
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
        private bool ImagesExists(int id)
        {
            return _context.CauseImages.Any(e => e.Id == id);
        }
    }
}
