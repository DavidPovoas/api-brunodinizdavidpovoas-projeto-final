using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Cache;
using MiniShop.Data;
using MiniShop.Models;

namespace MiniShop.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HybridCacheService _cache;

        public ProductsController(AppDbContext context, HybridCacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var cached = await _cache.GetAsync<List<Product>>("products:all");
            if (cached != null) return cached;
            var products = await _context.Products.ToListAsync();
            await _cache.SetAsync("products:all", products);
            return products;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var cached = await _cache.GetAsync<Product>($"products:{id}");
            if (cached != null) return cached;
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            await _cache.SetAsync($"products:{id}", product);
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync("products:all");
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"products:{id}");
            await _cache.RemoveAsync("products:all");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"products:{id}");
            await _cache.RemoveAsync("products:all");
            return NoContent();
        }
    }
}