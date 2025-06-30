using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuotesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Quotes>>> GetQuotes()
        {
            try
            {
                var quotes = await _context.Quotes
                    .Include(q => q.Client)
                    .Include(q => q.Availability)
                    .Include(q => q.User)
                    .ToListAsync();
                Console.WriteLine($"Citas cargadas: {quotes.Count}");
                return quotes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetQuotes: {ex.Message}");
                return StatusCode(500, new { Message = "Error loading appointments", Error = ex.Message });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Quotes>>> GetUserQuotes()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new { Message = "User email not found in token." });
                }

                var quotes = await _context.Quotes
                    .Include(q => q.Client)
                    .Include(q => q.Availability)
                    .Include(q => q.User)
                    .Where(q => q.Client.Email == userEmail && q.IsActive)
                    .ToListAsync();
                Console.WriteLine($"Citas cargadas para el usuario {userEmail}: {quotes.Count}");
                return quotes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUserQuotes: {ex.Message}");
                return StatusCode(500, new { Message = "Error loading user appointments", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Quotes>> GetQuotes(int id)
        {
            try
            {
                var quote = await _context.Quotes
                    .Include(q => q.Client)
                    .Include(q => q.Availability)
                    .Include(q => q.User)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quote == null)
                {
                    Console.WriteLine($"Cita no encontrada: Id = {id}");
                    return NotFound(new { Message = "Appointment not found." });
                }

                return quote;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetQuotes/{id}: {ex.Message}");
                return StatusCode(500, new { Message = "Error loading appointment", Error = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Quotes>> PostQuote([FromBody] QuoteDto quoteDto)
        {
            try
            {
                // Validar datos obligatorios
                if (string.IsNullOrWhiteSpace(quoteDto.ClientName) ||
                    string.IsNullOrWhiteSpace(quoteDto.ClientEmail) ||
                    string.IsNullOrWhiteSpace(quoteDto.ClientPhone))
                {
                    return BadRequest(new { Message = "Name, email, and phone are required." });
                }

                // Verificar si el usuario está autenticado y validar el correo
                if (User.Identity.IsAuthenticated)
                {
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (string.IsNullOrEmpty(userEmail))
                    {
                        return Unauthorized(new { Message = "User email not found in token." });
                    }
                    if (quoteDto.ClientEmail != userEmail)
                    {
                        return BadRequest(new { Message = "Provided email does not match authenticated user's email." });
                    }
                }

                var availability = await _context.Availability.FindAsync(quoteDto.AvailabilityId);
                if (availability == null || !availability.Available)
                {
                    return BadRequest(new { Message = "Selected time slot is not available." });
                }

                if (availability.Hour < new TimeSpan(7, 0, 0) || availability.Hour > new TimeSpan(17, 0, 0))
                {
                    return BadRequest(new { Message = "Appointments must be between 7:00 AM and 7:00 PM." });
                }

                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == quoteDto.ClientEmail);
                if (client == null)
                {
                    client = new Clients
                    {
                        Name = quoteDto.ClientName,
                        Email = quoteDto.ClientEmail,
                        Phone = quoteDto.ClientPhone
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                }

                int? userId = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var quote = new Quotes
                {
                    Notes = quoteDto.Notes ?? "",
                    IsActive = true,
                    ClienteId = client.Id,
                    AvailabilityID = quoteDto.AvailabilityId,
                    UserId = userId
                };

                availability.Available = false;
                _context.Quotes.Add(quote);
                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Cambios guardados en PostQuote: {changes}");

                return CreatedAtAction("GetQuotes", new { id = quote.Id }, quote);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PostQuote: {ex.Message}");
                return StatusCode(500, new { Message = "Error scheduling appointment", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutQuote(int id, [FromBody] QuoteDto quoteDto)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new { Message = "User email not found in token." });
                }

                var quote = await _context.Quotes
                    .Include(q => q.Client)
                    .Include(q => q.Availability)
                    .FirstOrDefaultAsync(q => q.Id == id);
                if (quote == null)
                {
                    Console.WriteLine($"Cita no encontrada para PutQuote: Id = {id}");
                    return NotFound(new { Message = "Appointment not found." });
                }

                // Verificar si el usuario es el propietario de la cita o un administrador
                if (quote.Client.Email != userEmail && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only update your own appointments.");
                }

                if (string.IsNullOrWhiteSpace(quoteDto.ClientName) ||
                    string.IsNullOrWhiteSpace(quoteDto.ClientEmail) ||
                    string.IsNullOrWhiteSpace(quoteDto.ClientPhone))
                {
                    return BadRequest(new { Message = "Name, email, and phone are required." });
                }

                if (quoteDto.ClientEmail != userEmail && !User.IsInRole("Admin"))
                {
                    return BadRequest(new { Message = "Provided email does not match authenticated user's email." });
                }

                var newAvailability = await _context.Availability.FindAsync(quoteDto.AvailabilityId);
                if (newAvailability == null || !newAvailability.Available)
                {
                    return BadRequest(new { Message = "Selected time slot is not available." });
                }

                if (newAvailability.Hour < new TimeSpan(7, 0, 0) || newAvailability.Hour > new TimeSpan(17, 0, 0))
                {
                    return BadRequest(new { Message = "Appointments must be between 7:00 AM and 7:00 PM." });
                }

                var oldAvailability = await _context.Availability.FindAsync(quote.AvailabilityID);
                if (oldAvailability != null && oldAvailability.Id != newAvailability.Id)
                {
                    oldAvailability.Available = true;
                    _context.Availability.Update(oldAvailability);
                }

                var client = await _context.Clients.FindAsync(quote.ClienteId);
                if (client != null)
                {
                    client.Name = quoteDto.ClientName;
                    client.Email = quoteDto.ClientEmail;
                    client.Phone = quoteDto.ClientPhone;
                    _context.Clients.Update(client);
                }

                quote.Notes = quoteDto.Notes ?? "";
                quote.AvailabilityID = quoteDto.AvailabilityId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                quote.UserId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (int?)null;
                newAvailability.Available = false;
                _context.Availability.Update(newAvailability);

                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Cambios guardados en PutQuote: {changes}");

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PutQuote: {ex.Message}");
                return StatusCode(500, new { Message = "Error updating appointment", Error = ex.Message });
            }
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            try
            {
                int changes = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_MarkQuoteAsCompleted @QuoteId",
                    new SqlParameter("@QuoteId", id));

                Console.WriteLine($"Cambios guardados en MarkAsCompleted: {changes}");
                if (changes == 0)
                {
                    Console.WriteLine($"Cita no encontrada para MarkAsCompleted: Id = {id}");
                    return NotFound(new { Message = "Appointment not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en MarkAsCompleted: {ex.Message}");
                return StatusCode(500, new { Message = "Error marking appointment as completed", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuote(int id)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new { Message = "User email not found in token." });
                }

                var quote = await _context.Quotes
                    .Include(q => q.Client)
                    .FirstOrDefaultAsync(q => q.Id == id);
                if (quote == null)
                {
                    Console.WriteLine($"Cita no encontrada para DeleteQuote: Id = {id}");
                    return NotFound(new { Message = "Appointment not found." });
                }

                if (quote.Client.Email != userEmail && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only delete your own appointments.");
                }

                int changes = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_DeleteQuote @QuoteId",
                    new SqlParameter("@QuoteId", id));

                Console.WriteLine($"Cambios guardados en DeleteQuote: {changes}");
                if (changes == 0)
                {
                    Console.WriteLine($"Cita no encontrada para DeleteQuote: Id = {id}");
                    return NotFound(new { Message = "Appointment not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DeleteQuote: {ex.Message}");
                return StatusCode(500, new { Message = "Error deleting appointment", Error = ex.Message });
            }
        }
    }

    public class QuoteDto
    {
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public int AvailabilityId { get; set; }
        public string Notes { get; set; }
    }
}
