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
    public class DiseasesNeedsPesticidesController : ControllerBase
    {
        private readonly DataContext _context;

        public DiseasesNeedsPesticidesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/DiseasesNeedsPesticides
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiseasesNeedsPesticides>>> GetDiseasesNeedsPesticides()
        {
            return await _context.DiseasesNeedsPesticides.ToListAsync();
        }

        // GET: api/DiseasesNeedsPesticides/5
        [HttpGet("{did}/{pid}")]
        public async Task<ActionResult<DiseasesNeedsPesticides>> GetDiseasesNeedsPesticides(long diseaseId, long pesticideId)
        {
            var diseasesNeedsPesticides = await _context.DiseasesNeedsPesticides.FindAsync(diseaseId, pesticideId);

            if (diseasesNeedsPesticides == null)
            {
                return NotFound();
            }

            return diseasesNeedsPesticides;
        }

        // POST: api/DiseasesNeedsPesticides
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DiseasesNeedsPesticides>> PostDiseasesNeedsPesticides(DiseasesNeedsPesticides diseasesNeedsPesticides)
        {
            _context.DiseasesNeedsPesticides.Add(diseasesNeedsPesticides);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DiseasesNeedsPesticidesExists(diseasesNeedsPesticides.DiseaseId, diseasesNeedsPesticides.PesticideId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDiseasesNeedsPesticides", new { did = diseasesNeedsPesticides.DiseaseId, pid = diseasesNeedsPesticides.PesticideId }, diseasesNeedsPesticides);
        }

        // DELETE: api/DiseasesNeedsPesticides/5
        [HttpDelete("{did}/{pid}")]
        public async Task<IActionResult> DeleteDiseasesNeedsPesticides(long did, long pid)
        {
            var diseasesNeedsPesticides = await _context.DiseasesNeedsPesticides.FindAsync(did, pid);
            if (diseasesNeedsPesticides == null)
            {
                return NotFound();
            }

            _context.DiseasesNeedsPesticides.Remove(diseasesNeedsPesticides);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiseasesNeedsPesticidesExists(long did, long pid)
        {
            return _context.DiseasesNeedsPesticides.Any(e =>
                    e.DiseaseId == did &&
                    e.PesticideId == pid);
        }
    }
}
