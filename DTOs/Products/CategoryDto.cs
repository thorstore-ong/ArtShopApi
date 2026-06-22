using ArtShopApi.Models;

namespace ArtShopApi.DTOs.Products
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string ? Name { get; set; }

        public static CategoryDto FromCategory(Category category) => new() { 
            Id = category.Id,
            Name = category.Name
        };

    }
}
