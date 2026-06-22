using ArtShopApi.Models;

namespace ArtShopApi.DTOs.Order
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
