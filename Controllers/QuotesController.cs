using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public QuotesController(AppDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: api/Quotes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quotes>>> GetQuotes()
        {
            return await _context.Quotes.ToListAsync();
        }

        // GET: api/Quotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quotes>> GetQuotes(int id)
        {
            var quotes = await _context.Quotes.FindAsync(id);

            if (quotes == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            return quotes;
        }

        // PUT: api/Quotes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuotes(int id, Quotes quotes)
        {
            if (id != quotes.Id)
            {
                return BadRequest(new { message = _localizer["InvalidRequest"] });
            }

            _context.Entry(quotes).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuotesExists(id))
                {
                    return NotFound(new { message = _localizer["NotFound"] });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = _localizer["Updated"] });
        }

        // POST: api/Quotes
        [HttpPost]
        public async Task<ActionResult<Quotes>> PostQuotes(Quotes quotes)
        {
            _context.Quotes.Add(quotes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuotes", new { id = quotes.Id }, new
            {
                message = _localizer["Created"],
                data = quotes
            });
        }

        // DELETE: api/Quotes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotes(int id)
        {
            var quotes = await _context.Quotes.FindAsync(id);
            if (quotes == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            _context.Quotes.Remove(quotes);
            await _context.SaveChangesAsync();

            return Ok(new { message = _localizer["Deleted"] });
        }

        private bool QuotesExists(int id)
        {
            return _context.Quotes.Any(e => e.Id == id);
        }
    }
}
