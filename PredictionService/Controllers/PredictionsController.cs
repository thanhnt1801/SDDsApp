using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PredictionService.Data;
using PredictionService.Models;

namespace PredictionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionsController : ControllerBase
    {
        private readonly DataContext _context;

        public PredictionsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Predictions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prediction>>> GetPredictions()
        {
            return await _context.Predictions.ToListAsync();
        }

        // GET: api/Predictions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prediction>> GetPrediction(long id)
        {
            var prediction = await _context.Predictions.FindAsync(id);

            if (prediction == null)
            {
                return NotFound();
            }

            return prediction;
        }

        // PUT: api/Predictions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrediction(long id, Prediction prediction)
        {
            if (id != prediction.Id)
            {
                return BadRequest();
            }

            _context.Entry(prediction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PredictionExists(id))
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

        // POST: api/Predictions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Prediction>> PostPrediction(Prediction prediction)
        {
            _context.Predictions.Add(prediction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPrediction", new { id = prediction.Id }, prediction);
        }

        // DELETE: api/Predictions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrediction(long id)
        {
            var prediction = await _context.Predictions.FindAsync(id);
            if (prediction == null)
            {
                return NotFound();
            }

            _context.Predictions.Remove(prediction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PredictionExists(long id)
        {
            return _context.Predictions.Any(e => e.Id == id);
        }

        [HttpGet("GetHistoryByFarmer/{farmerId}")]
        public async Task<ActionResult<IEnumerable<Prediction>>> GetHistoryByFarmer(Guid farmerId)
        {
            var history = await _context.Predictions.Where(h => h.FarmerId == farmerId).ToListAsync();
            return history;
        }
        
        [HttpGet("GetHistoryByExpert/{expertId}")]
        public async Task<ActionResult<IEnumerable<Prediction>>> GetHistoryByExpert(Guid expertId)
        {
            var history = await _context.Predictions.Where(h => h.ExpertId == expertId).ToListAsync();
            return history;
        }

    }
}
