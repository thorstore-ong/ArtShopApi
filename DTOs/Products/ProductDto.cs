using ArtShopApi.Models;

namespace ArtShopApi.DTOs.Products
{
    public class ProductDto
    {
        public int ? Id { get; set; }
        public string? Name { get; set; }
        public string ? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ? ImageUrl {  get; set; }
        public string ? CategoryName { get; set; }

        public static ProductDto FromProduct(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }
}
