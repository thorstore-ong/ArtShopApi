using Microsoft.AspNetCore.Mvc;
using ArtShopApi.Data;
using ArtShopApi.DTOs.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ArtShopApi.Models;

namespace ArtShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return Ok(products.Select(ProductDto.FromProduct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id) {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            return Ok(ProductDto.FromProduct(product));
        }

        //Where customers can find products via category
        [HttpGet("{name}/products")]
        public async Task<ActionResult<ProductDto>> GetProductsByCategory(string name)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
            if (category == null)
                return BadRequest("Category not Found.");

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == category.Id)
                .ToListAsync();

            return Ok(products.Select(ProductDto.FromProduct));
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Category does not exist");


            var product = new Product
            {
                Name = dto.Name!,
                Description = dto.Description!,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = dto.ImageUrl!,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return Ok(ProductDto.FromProduct(product));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, CreateProductDto dto)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            product.Name = dto.Name!;
            product.Description = dto.Description!;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.ImageUrl = dto.ImageUrl!;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return Ok(ProductDto.FromProduct(product));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> DeleteProduct(int id) { 

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
            
        }
    }
}
