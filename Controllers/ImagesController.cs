using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public ImagesController(AppDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Images>>> GetImages()
        {
            return await _context.Images.ToListAsync();
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Images>> GetImage(int id)
        {
            var image = await _context.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            return image;
        }

        // PUT: api/Images/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(
            int id,
            [FromForm] int type,
            [FromForm] IFormFile image)
        {
            var img = await _context.Images.FindAsync(id);

            if (img == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            img.Type = type;

            if (image != null)
            {
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(imagesPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                img.Route = "/images/" + fileName;
            }

            _context.Entry(img).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImagesExists(id))
                {
                    return NotFound(new { message = _localizer["NotFound"] });
                }
                throw;
            }

            return Ok(new { message = _localizer["Updated"] });
        }

        // POST: api/Images
        [HttpPost]
        public async Task<ActionResult<Images>> CreateImage([FromForm] int type, [FromForm] IFormFile image)
        {
            var img = new Images { Type = type };

            if (image != null)
            {
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(imagesPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                img.Route = "/images/" + fileName;
            }

            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetImage), new { id = img.Id }, new { message = _localizer["Created"] });
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = _localizer["Deleted"] });
        }

        private bool ImagesExists(int id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
    }
}
