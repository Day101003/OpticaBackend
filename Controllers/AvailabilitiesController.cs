using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AvailabilitiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Availabilities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Availability>>> GetAvailability()
        {
            return await _context.Availability.ToListAsync();
        }

        // GET: api/Availabilities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Availability>> GetAvailability(int id)
        {
            var availability = await _context.Availability.FindAsync(id);

            if (availability == null)
            {
                return NotFound();
            }

            return availability;
        }

        // POST: api/Availabilities
        [HttpPost]
        public async Task<ActionResult<Availability>> PostAvailability(Availability availability)
        {
            _context.Availability.Add(availability);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAvailability", new { id = availability.Id }, availability);
        }

        // PUT: api/Availabilities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvailability(int id, Availability availability)
        {
            if (id != availability.Id)
            {
                return BadRequest();
            }

            _context.Entry(availability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityExists(id))
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

        // DELETE: api/Availabilities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var availability = await _context.Availability.FindAsync(id);
            if (availability == null)
            {
                return NotFound();
            }

            _context.Availability.Remove(availability);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Availabilities/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAvailabilities()
        {
            try
            {
                var now = DateTime.Now;
                var startDate = new DateTime(now.Year, now.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var slots = new List<Availability>();
                var hours = new TimeSpan[] {
                    new TimeSpan(7, 0, 0),  // 7:00 AM
                    new TimeSpan(9, 0, 0),  // 9:00 AM
                    new TimeSpan(11, 0, 0), // 11:00 AM
                    new TimeSpan(13, 0, 0), // 1:00 PM
                    new TimeSpan(15, 0, 0), // 3:00 PM
                    new TimeSpan(17, 0, 0)  // 5:00 PM
                };

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Opcional: Excluir fines de semana
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    foreach (var hour in hours)
                    {
                        // Verificar si el slot ya existe
                        var exists = await _context.Availability
                            .AnyAsync(a => a.AvailableDate.Date == date.Date && a.Hour == hour);

                        if (!exists)
                        {
                            slots.Add(new Availability
                            {
                                AvailableDate = date,
                                Hour = hour,
                                Available = true
                            });
                        }
                    }
                }

                _context.Availability.AddRange(slots);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"{slots.Count} slots generated for {startDate:MMMM yyyy}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error generating availabilities", Error = ex.Message });
            }
        }

        // GET: api/Availabilities/available/{year}/{month}
        [HttpGet("available/{year}/{month}")]
        public async Task<IActionResult> GetAvailableSlots(int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var slots = await _context.Availability
                    .Where(a => a.AvailableDate >= startDate && a.AvailableDate <= endDate && a.Available)
                    .Select(a => new { a.Id, a.AvailableDate, a.Hour })
                    .OrderBy(a => a.AvailableDate)
                    .ThenBy(a => a.Hour)
                    .ToListAsync();

                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving available slots", Error = ex.Message });
            }
        }

        private bool AvailabilityExists(int id)
        {
            return _context.Availability.Any(e => e.Id == id);
        }
    }
}