﻿using System;
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
    public class PreventativeMeasuresController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PreventativeMeasuresController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/PreventativeMeasures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PreventativeMeasureDTO>>> GetPreventativeMeasures()
        {
            var listPreventativeMeasure = await _context.PreventativeMeasures.ToListAsync();
            var listPreventativeMeasureDTO = _mapper.Map<IEnumerable<PreventativeMeasureDTO>>(listPreventativeMeasure);
            return listPreventativeMeasureDTO.ToList();
        }

        // GET: api/PreventativeMeasures/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PreventativeMeasure>> GetPreventativeMeasure(long id)
        {
            var preventativeMeasure = await _context.PreventativeMeasures.Include(ds => ds.PreventativeMeasureImages).SingleOrDefaultAsync(ds => ds.Id == id);

            if (preventativeMeasure == null)
            {
                return NotFound();
            }

            return preventativeMeasure;
        }

        [HttpGet("GetImages/{id}")]
        public async Task<ActionResult<List<PreventativeMeasureImages>>> GetImages(long id)
        {
            var images = await _context.PreventativeMeasureImages.Where(cs => cs.PreventativeMeasureId == id).ToListAsync();

            if (images == null)
            {
                return NotFound();
            }

            return images;
        }

        // PUT: api/PreventativeMeasures/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPreventativeMeasure(long id, PreventativeMeasure preventativeMeasure)
        {
            if (id != preventativeMeasure.Id)
            {
                return BadRequest();
            }

            _context.Entry(preventativeMeasure).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PreventativeMeasureExists(id))
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

        // POST: api/PreventativeMeasures
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PreventativeMeasureDTO>> PostPreventativeMeasure(PreventativeMeasure preventativeMeasure)
        {
            _context.PreventativeMeasures.Add(preventativeMeasure);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPreventativeMeasure", new { id = preventativeMeasure.Id }, preventativeMeasure);
        }

        [HttpPost("PostPreventativeMeasureImages")]
        public async Task<ActionResult<PreventativeMeasureImages>> PostPreventativeMeasureImages(PreventativeMeasureImages preventativeMeasureImages)
        {
            _context.PreventativeMeasureImages.Add(preventativeMeasureImages);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPreventativeMeasure", new { id = preventativeMeasureImages.Id }, preventativeMeasureImages);
        }

        // DELETE: api/PreventativeMeasures/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreventativeMeasure(long id)
        {
            var preventativeMeasure = await _context.PreventativeMeasures.FindAsync(id);
            if (preventativeMeasure == null)
            {
                return NotFound();
            }

            preventativeMeasure.Status = false;

            _context.Entry(preventativeMeasure).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PreventativeMeasureExists(id))
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
            var images = await _context.PreventativeMeasureImages.FindAsync(id);
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

        private bool PreventativeMeasureExists(long id)
        {
            return _context.PreventativeMeasures.Any(e => e.Id == id);
        }
        private bool ImagesExists(int id)
        {
            return _context.PreventativeMeasureImages.Any(e => e.Id == id);
        }
    }
}
