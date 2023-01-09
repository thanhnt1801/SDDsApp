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
    public class DiseasesNeedsMeasuresController : ControllerBase
    {
        private readonly DataContext _context;

        public DiseasesNeedsMeasuresController(DataContext context)
        {
            _context = context;
        }

        // GET: api/DiseasesNeedsMeasures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiseasesNeedsMeasures>>> GetDiseasesNeedsMeasures()
        {
            return await _context.DiseasesNeedsMeasures.ToListAsync();
        }

        // GET: api/DiseasesNeedsMeasures/5
        [HttpGet("{did}/{mid}")]
        public async Task<ActionResult<DiseasesNeedsMeasures>> GetDiseasesNeedsMeasures(long diseaseId, long measureId)
        {
            var diseasesNeedsMeasures = await _context.DiseasesNeedsMeasures.FindAsync(diseaseId, measureId);

            if (diseasesNeedsMeasures == null)
            {
                return NotFound();
            }

            return diseasesNeedsMeasures;
        }

        [HttpPost]
        public async Task<ActionResult<DiseasesNeedsMeasures>> PostDiseasesNeedsMeasures(DiseasesNeedsMeasures diseasesNeedsMeasures)
        {
            _context.DiseasesNeedsMeasures.Add(diseasesNeedsMeasures);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DiseasesNeedsMeasuresExists(diseasesNeedsMeasures.DiseaseId, diseasesNeedsMeasures.PreventativeMeasureId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDiseasesNeedsMeasures", new { did = diseasesNeedsMeasures.DiseaseId, mid = diseasesNeedsMeasures.PreventativeMeasureId }, diseasesNeedsMeasures);
        }

        // DELETE: api/DiseasesNeedsMeasures/5
        [HttpDelete("{diseaseId}/{measureId}")]
        public async Task<IActionResult> DeleteDiseasesNeedsMeasures(long diseaseId, long measureId)
        {
            var diseasesNeedsMeasures = await _context.DiseasesNeedsMeasures.FindAsync(diseaseId, measureId);
            if (diseasesNeedsMeasures == null)
            {
                return NotFound();
            }

            _context.DiseasesNeedsMeasures.Remove(diseasesNeedsMeasures);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiseasesNeedsMeasuresExists(long diseaseId, long measureId)
        {
            return _context.DiseasesNeedsMeasures.Any(e =>
                    e.DiseaseId == diseaseId &&
                    e.PreventativeMeasureId == measureId);
        }
    }
}
