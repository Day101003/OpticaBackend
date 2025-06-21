using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Products productData)
        {
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Code = productData.Code;
            existingProduct.Name = productData.Name;
            existingProduct.Description = productData.Description;
            existingProduct.Price = productData.Price;
            existingProduct.IsActive = productData.IsActive;
            existingProduct.CategoriaId = productData.CategoriaId;
            existingProduct.ImageId = productData.ImageId;

            _context.Entry(existingProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductsExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Products>> CreateProduct([FromBody] Products product)
        {
            if (product == null)
                return BadRequest("Producto vacío.");

            if (string.IsNullOrWhiteSpace(product.Name) ||
    product.CategoriaId == 0 || product.CategoriaId == null ||
    product.ImageId == 0 || product.ImageId == null ||
    product.Price == null || product.Price <= 0)
            {
                return BadRequest("Faltan datos obligatorios o datos inválidos.");
            }


            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoriaId);
            var imageExists = await _context.Images.AnyAsync(i => i.Id == product.ImageId);

            if (!categoryExists)
                return BadRequest($"No existe la categoría con Id {product.CategoriaId}.");

            if (!imageExists)
                return BadRequest($"No existe la imagen con Id {product.ImageId}.");

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                // Devuelve error con mensaje
                return BadRequest($"Error al guardar el producto: {ex.Message}");
            }
        }


        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
