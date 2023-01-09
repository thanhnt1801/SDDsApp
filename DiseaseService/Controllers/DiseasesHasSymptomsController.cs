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
    public class DiseasesHasSymptomsController : ControllerBase
    {
        private readonly DataContext _context;

        public DiseasesHasSymptomsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/DiseasesHasSymptoms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiseasesHasSymptoms>>> GetDiseasesHasSymptoms()
        {
            return await _context.DiseasesHasSymptoms.ToListAsync();
        }

        // GET: api/DiseasesHasSymptoms/5/1
        [HttpGet("{did}/{sid}")]
        public async Task<ActionResult<DiseasesHasSymptoms>> GetDiseasesHasSymptoms(long diseaseId, long symptomId)
        {
            var diseasesHasSymptoms = await _context.DiseasesHasSymptoms.FindAsync(diseaseId, symptomId);

            if (diseasesHasSymptoms == null)
            {
                return NotFound();
            }

            return diseasesHasSymptoms;
        }

        // POST: api/DiseasesHasSymptoms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DiseasesHasSymptoms>> PostDiseasesHasSymptoms(DiseasesHasSymptoms diseasesHasSymptoms)
        {
            _context.DiseasesHasSymptoms.Add(diseasesHasSymptoms);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DiseasesHasSymptomsExists(diseasesHasSymptoms.DiseaseId, diseasesHasSymptoms.SymptomId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDiseasesHasSymptoms", new { did = diseasesHasSymptoms.DiseaseId, cid = diseasesHasSymptoms.SymptomId }, diseasesHasSymptoms);
        }

        // DELETE: api/DiseasesHasSymptoms/5/1
        [HttpDelete("{diseaseId}/{symptomId}")]
        public async Task<IActionResult> DeleteDiseasesHasSymptoms(long diseaseId, long symptomId)
        {
            var diseasesHasSymptoms = await _context.DiseasesHasSymptoms.FindAsync(diseaseId, symptomId);
            if (diseasesHasSymptoms == null)
            {
                return NotFound();
            }

            _context.DiseasesHasSymptoms.Remove(diseasesHasSymptoms);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiseasesHasSymptomsExists(long diseaseId, long symptomId)
        {
            return _context.DiseasesHasSymptoms.Any(e => 
                    e.DiseaseId == diseaseId && 
                    e.SymptomId == symptomId);
        }
    }
}
