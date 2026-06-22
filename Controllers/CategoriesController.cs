using ArtShopApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtShopApi.DTOs.Products;
using Microsoft.AspNetCore.Authorization;
using ArtShopApi.Models;

namespace ArtShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context) {

            _context = context;
        }
        //Displays all Categories to choose from
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories.Select(CategoryDto.FromCategory));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<CategoryDto>>> GetById(int id) {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            return Ok(CategoryDto.FromCategory(category));
        }
          

        //This is where the admin of the shop can create a new Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto dto)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                return BadRequest("Category Exists");

            var category = new Category
            {
                Name = dto.Name!,
                
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(CategoryDto.FromCategory(category));
            
        }
    }
}
