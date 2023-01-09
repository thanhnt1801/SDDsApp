using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiseaseService.Data;
using DiseaseService.Models;
using DiseaseService.DTOs;
using AutoMapper;
using Microsoft.CodeAnalysis;

namespace DiseaseService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiseasesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public DiseasesController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Diseases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Disease>>> GetDiseases()
        {
            var listDisease = await _context.Diseases
                .Include(d => d.DiseasesHasSymptoms)
                .ThenInclude(s => s.Symptom)
                .Include(d => d.DiseasesNeedsMeasures)
                .ThenInclude(m => m.PreventativeMeasure)
                .Include(d => d.DiseasesNeedsPesticides)
                .ThenInclude(p => p.Pesticide)
                .ToListAsync();
            return listDisease;
        }

        // GET: api/Diseases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Disease>> GetDisease(long id)
        {
            var disease = await _context.Diseases.FindAsync(id);

            if (disease == null)
            {
                return NotFound();
            }

            var diseaseDTO = _mapper.Map<Disease>(disease);

            return diseaseDTO;
        }

        [HttpGet("{diseaseId}/Cause")]
        public ActionResult<Cause> GetCausesByDisease(long diseaseId)
        {
            var symptom = _context.DiseasesHasCauses.Where(o => o.DiseaseId == diseaseId).Select(p => p.Cause).ToList();
            if (symptom == null) return NotFound();
            return Ok(symptom);
        }

        [HttpGet("GetRestCauses/{diseaseId}")]
        public ActionResult<Symptom> GetRestCauses(long diseaseId)
        {
            var causes = _context.Causes
                            .Include(s => s.DiseasesHasCauses)
                            .Where(x => !x.DiseasesHasCauses.Any()
                            || !x.DiseasesHasCauses.Any(ds => ds.DiseaseId == diseaseId))
                            .ToList();
            if (causes == null) return NotFound();
            return Ok(causes);
        }


        [HttpGet("{diseaseId}/Symptom")]
        public ActionResult<Symptom> GetSymptomsByDisease(long diseaseId)
        {
            var symptom = _context.DiseasesHasSymptoms.Where(o => o.DiseaseId == diseaseId).Select(p => p.Symptom).ToList();
            if (symptom == null) return NotFound();
            return Ok(symptom);
        }

        [HttpGet("GetRestSymptoms/{diseaseId}")]
        public ActionResult<Symptom> GetRestSymptoms(long diseaseId)
        {
            var symptoms = _context.Symptoms
                            .Include(s => s.DiseasesHasSymptoms)
                            .Where(x => !x.DiseasesHasSymptoms.Any()
                            || !x.DiseasesHasSymptoms.Any(ds => ds.DiseaseId == diseaseId))
                            .ToList();
            if (symptoms == null) return NotFound();
            return Ok(symptoms);
        }


        [HttpGet("{diseaseId}/Pesticide")]
        public ActionResult<Symptom> GetPesticidesByDisease(long diseaseId)
        {
            var symptom = _context.DiseasesNeedsPesticides.Where(o => o.DiseaseId == diseaseId).Select(p => p.Pesticide).ToList();
            if (symptom == null) return NotFound();
            return Ok(symptom);
        }

        [HttpGet("GetRestPesticides/{diseaseId}")]
        public ActionResult<Pesticide> GetRestPesticides(long diseaseId)
        {
            var pesticides = _context.Pesticides
                            .Include(s => s.DiseasesNeedsPesticides)
                            .Where(x => !x.DiseasesNeedsPesticides.Any()
                            || !x.DiseasesNeedsPesticides.Any(ds => ds.DiseaseId == diseaseId))
                            .ToList();
            if (pesticides == null) return NotFound();
            return Ok(pesticides);
        }


        [HttpGet("{diseaseId}/Measure")]
        public ActionResult<Symptom> GetMeasuresByDisease(long diseaseId)
        {
            var symptom = _context.DiseasesNeedsMeasures.Where(o => o.DiseaseId == diseaseId).Select(p => p.PreventativeMeasure).ToList();
            if (symptom == null) return NotFound();
            return Ok(symptom);
        }

        [HttpGet("GetRestMeasures/{diseaseId}")]
        public ActionResult<PreventativeMeasure> GetRestMeasures(long diseaseId)
        {
            var measures = _context.PreventativeMeasures
                         .Include(s => s.DiseasesNeedsMeasures)
                         .Where(x => !x.DiseasesNeedsMeasures.Any()
                         || !x.DiseasesNeedsMeasures.Any(ds => ds.DiseaseId == diseaseId))
                         .ToList();
            if (measures == null) return NotFound();
            return Ok(measures);
        }


        // PUT: api/Diseases/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDisease(long id, DiseaseDTO diseaseDTO)
        {            
            if (id != diseaseDTO.Id)
            {
                return BadRequest();
            }

            var disease = _mapper.Map<Disease>(diseaseDTO);

            _context.Entry(disease).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiseaseExists(id))
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

        // POST: api/Diseases
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DiseaseDTO>> PostDisease(Disease disease)
        {
            if (_context.Diseases.Any(d => d.Id == disease.Id))
            {
                return BadRequest("Disease Already Exist!");
            }

            _context.Entry(disease).State = EntityState.Modified;

            try
            {
                _context.Diseases.Add(disease);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Something is wrong when trying to Create disease!");
            }
            return CreatedAtAction("GetDisease", new { id = disease.Id }, disease);
        }

        // DELETE: api/Diseases/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisease(long id)
        {
            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null)
            {
                return NotFound();
            }

            disease.Status = false;

            _context.Entry(disease).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiseaseExists(id))
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

        private bool DiseaseExists(long id)
        {
            return _context.Diseases.Any(e => e.Id == id);
        }

        [HttpGet("search")]
        public  List<Disease> SearchDisease(string query)
        {
            List<Disease> d = new List<Disease>();
            try
            {
              d = _context.Diseases.Where(d => d.Name.Trim().ToUpper().Contains(query.TrimEnd().ToUpper())).ToList();
             
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            return d;
        }
    }
}
