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

        [HttpGet("ExpertConfirmation/predictionId={predictionId}")]
        public async Task<IActionResult> ExpertConfirmation(long predictionId, string confirm)
        {
            var prediction = await _context.Predictions.FindAsync(predictionId);
            if (prediction == null)
            {
                return BadRequest("Prediction Does Not Exist!");
            }

            if (confirm == "true")
            {
                prediction.ExpertConfirmation = "The diagnosis was correct!";
                prediction.UpdatedAt = DateTime.Now;
                prediction.Status = true;
            }
            else
            {
                prediction.ExpertConfirmation = "The diagnosis was incorrect! Correct Disease is " + confirm;
                prediction.UpdatedAt = DateTime.Now;
                prediction.Status = true;
            }
            _context.Update(prediction);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return Ok(prediction.ExpertConfirmation);
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

        [HttpGet("GetQueueOfPrediction/{expertId}")]
        public async Task<ActionResult<IEnumerable<Prediction>>> GetHistoryByExpert(Guid expertId)
        {
            var history = await _context.Predictions.Where(pre => pre.ExpertId.Equals(expertId)).ToListAsync();
            return history;
        }

        [HttpGet("CorrectDiagnosisPercent")]
        public IActionResult CorrectDiagnosisPercent()
        {
            float totalPrediction = _context.Predictions
                .Where(pre => pre.ExpertConfirmation
                    .Equals("The diagnosis was correct!"))
                .Count();

            float healthyCorrectDiagnosis = _context.Predictions
                .Where(pre => pre.ExpertConfirmation
                    .Equals("The diagnosis was correct!")
                    && pre.PredictResult.Equals("Strawberry Healthy Leaf"))
                .Count();

            float leafSpotCorrectDiagnosis = _context.Predictions
                .Where(pre => pre.ExpertConfirmation
                    .Equals("The diagnosis was correct!")
                    && pre.PredictResult.Equals("Strawberry Leaf Spot"))
                .Count();

            float powerdyCorrectDiagnosis = _context.Predictions
                .Where(pre => pre.ExpertConfirmation
                    .Equals("The diagnosis was correct!")
                    && pre.PredictResult.Equals("Strawberry Powdery Mildew Leaf"))
                .Count();

            float healthPercent = (healthyCorrectDiagnosis / totalPrediction);
            float leafSpotPercent = (leafSpotCorrectDiagnosis / totalPrediction);
            float powderyPercent = (powerdyCorrectDiagnosis / totalPrediction);

            return Ok(new
            {
                healthPercent,
                leafSpotPercent,
                powderyPercent
            });
        }

        [HttpGet("TotalDiagnosisToday")]
        public IActionResult TotalDiagnosisToday()
        {
            var healthyDiagnosisToday = _context.Predictions
                .Where(pre => pre.CreatedAt.Date
                    .Equals(DateTime.Today.Date) && pre.PredictResult.Equals("Strawberry Healthy Leaf"))
                .Count();
            var leafSpotDiagnosisToday = _context.Predictions
                .Where(pre => pre.CreatedAt.Date
                    .Equals(DateTime.Today.Date) && pre.PredictResult.Equals("Strawberry Leaf Spot"))
                .Count();
            var powderyDiagnosisToday = _context.Predictions
                .Where(pre => pre.CreatedAt.Date
                    .Equals(DateTime.Today.Date) && pre.PredictResult.Equals("Strawberry Powdery Mildew Leaf"))
                .Count();


            return Ok(new
            {
                healthyDiagnosisToday,
                leafSpotDiagnosisToday,
                powderyDiagnosisToday
            });
        }
        [HttpGet("TotalDiagnosisLast7days")]
        public IActionResult TotalDiagnosisLast7days()
        {
            var listDays = new List<int>();
            var listPrediction = new List<int>();
            var last7days = _context.Predictions.Select(pre => pre.CreatedAt.Day).Distinct();
            foreach (var item in last7days)
            {
                listDays.Add(item);
            }
            if(listDays.Count > 7) {
                listDays.RemoveAt(0);
            }
            
            for(int i =0 ; i < listDays.Count; i++)
            {
                var prediction = _context.Predictions.Where(pre => pre.CreatedAt.Day.Equals(listDays[i])).Count();
                if(prediction != 0)
                {
                    listPrediction.Add(prediction);
                }
            }

            return Ok(new
            {
                listDays, listPrediction
            });
        }


    }
}
