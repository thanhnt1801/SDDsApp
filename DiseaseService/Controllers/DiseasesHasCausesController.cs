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
    public class DiseasesHasCausesController : ControllerBase
    {
        private readonly DataContext _context;

        public DiseasesHasCausesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/DiseasesHasCauses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiseasesHasCauses>>> GetDiseasesHasCauses()
        {
            return await _context.DiseasesHasCauses.ToListAsync();
        }

        // GET: api/DiseasesHasCauses/5
        [HttpGet("{diseaseId}/{causeId}")]
        public async Task<ActionResult<DiseasesHasCauses>> GetDiseasesHasCauses(long diseaseId, long causeId)
        {
            var diseasesHasCauses = await _context.DiseasesHasCauses.FindAsync(diseaseId, causeId);

            if (diseasesHasCauses == null)
            {
                return NotFound();
            }

            return diseasesHasCauses;
        }

        [HttpPost]
        public async Task<ActionResult<DiseasesHasCauses>> PostDiseasesHasCauses(DiseasesHasCauses diseasesHasCauses)
        {
            _context.DiseasesHasCauses.Add(diseasesHasCauses);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DiseasesHasCausesExists(diseasesHasCauses.DiseaseId, diseasesHasCauses.CauseId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            return CreatedAtAction("GetDiseasesHasCauses", new { did = diseasesHasCauses.DiseaseId, cid = diseasesHasCauses.CauseId }, diseasesHasCauses);
        }

        // DELETE: api/DiseasesHasCauses/5
        [HttpDelete("{diseaseId}/{causeId}")]
        public async Task<IActionResult> DeleteDiseasesHasCauses(long diseaseId, long causeId)
        {
            var diseasesHasCauses = await _context.DiseasesHasCauses.FindAsync(diseaseId, causeId);
            if (diseasesHasCauses == null)
            {
                return NotFound();
            }

            _context.DiseasesHasCauses.Remove(diseasesHasCauses);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiseasesHasCausesExists(long diseaseId, long causeId)
        {
            return _context.DiseasesHasCauses.Any(e =>
                    e.DiseaseId == diseaseId &&
                    e.CauseId == causeId);
        }
    }
}
