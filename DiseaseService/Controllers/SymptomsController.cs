using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiseaseService.Data;
using DiseaseService.Models;
using AutoMapper;
using DiseaseService.DTOs;

namespace DiseaseService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SymptomsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public SymptomsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Symptoms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SymptomDTO>>> GetSymptoms()
        {
            var listSymptom = await _context.Symptoms.ToListAsync();
            var listSymptomDTO = _mapper.Map<IEnumerable<SymptomDTO>>(listSymptom);
            return listSymptomDTO.ToList();
        }

        // GET: api/Symptoms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Symptom>> GetSymptom(long id)
        {
            var symptom = await _context.Symptoms.FindAsync(id);

            if (symptom == null)
            {
                return NotFound();
            }

            return symptom;
        }

        // PUT: api/Symptoms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSymptom(long id, Symptom symptom)
        {
            if (id != symptom.Id)
            {
                return BadRequest();
            }

            _context.Entry(symptom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SymptomExists(id))
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




        // POST: api/Symptoms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SymptomDTO>> PostSymptom(Symptom symptom)
        {
            _context.Symptoms.Add(symptom);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSymptom", new { id = symptom.Id }, symptom);
        }

        // DELETE: api/Symptoms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSymptom(long id)
        {
            var symptom = await _context.Symptoms.FindAsync(id);
            if (symptom == null)
            {
                return NotFound();
            }

            symptom.Status = false;

            _context.Entry(symptom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SymptomExists(id))
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

        private bool SymptomExists(long id)
        {
            return _context.Symptoms.Any(e => e.Id == id);
        }
    }
}
