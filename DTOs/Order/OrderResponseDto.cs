

namespace ArtShopApi.DTOs.Order
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();

        public static OrderResponseDto FromOrder(Models.Order order) => new()
        {
            Id = order.Id,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            ShippingCost = order.ShippingCost,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(OrderItemResponseDto.FromOrderItem).ToList()
        };
    }
}
