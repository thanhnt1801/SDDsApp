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
    public class PesticidesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PesticidesController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Pesticides
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PesticideDTO>>> GetPesticides()
        {
            var listPesticide = await _context.Pesticides.ToListAsync();
            var listPesticideDTO = _mapper.Map<IEnumerable<PesticideDTO>>(listPesticide);
            return listPesticideDTO.ToList();
        }

        // GET: api/Pesticides/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pesticide>> GetPesticide(long id)
        {
            var pesticide = await _context.Pesticides.FindAsync(id);

            if (pesticide == null)
            {
                return NotFound();
            }

            return pesticide;
        }

        // PUT: api/Pesticides/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPesticide(long id, PesticideDTO pesticideDTO)
        {
            if (id != pesticideDTO.Id)
            {
                return BadRequest();
            }

            var pesticide = _mapper.Map<Pesticide>(pesticideDTO);

            _context.Entry(pesticide).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PesticideExists(id))
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

        // POST: api/Pesticides
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PesticideDTO>> PostPesticide(Pesticide pesticide)
        {
            if (_context.Diseases.Any(d => d.Id == pesticide.Id))
            {
                return BadRequest("Disease Already Exist!");
            }

            _context.Entry(pesticide).State = EntityState.Modified;

            try
            {
                _context.Pesticides.Add(pesticide);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Something is wrong when trying to Create disease!");
            }


            return CreatedAtAction("GetPesticide", new { id = pesticide.Id }, pesticide);
        }

        // DELETE: api/Pesticides/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePesticide(long id)
        {
            var pesticide = await _context.Pesticides.FindAsync(id);
            if (pesticide == null)
            {
                return NotFound();
            }

            pesticide.Status = false;

            _context.Entry(pesticide).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PesticideExists(id))
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

        private bool PesticideExists(long id)
        {
            return _context.Pesticides.Any(e => e.Id == id);
        }
    }
}
