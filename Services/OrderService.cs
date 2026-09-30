using ArtShopApi.Data;
using ArtShopApi.DTOs.Order;
using ArtShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtShopApi.Services //created a service for Order because of all the complex business logic that happen with orders
{
    public class OrderService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        private const decimal ShippingCost = 120;
        
        public OrderService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<OrderResponseDto> PlaceOrderAsync(int userId, CreateOrderDto dto)
        {
            
            // loading all products in one query rather than one query per item
            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // Checking every requested product was found
            if (products.Count != dto.Items.Count)
                throw new Exception("One or more products were not found.");

            // Check stock for each item
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                if (product.Stock < item.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}.");
            }

            // calculate total from database prices, never from frontend
            var subtotal = dto.Items.Sum(item =>
            {
                var product = products.First(p => p.Id == item.ProductId);
                return product.Price * item.Quantity;
            });

            var total = subtotal + ShippingCost;

            // create the order
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = dto.ShippingAddress,
                ShippingCost = ShippingCost,
                Total = total,
                Status = "Pending"
            };

            // create order items, locking in the price at time of purchase
            order.OrderItems = dto.Items.Select(item =>
            {
                var product = products.First(p => p.Id == item.ProductId);
                return new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price  // snapshot of price right now
                };
            }).ToList();

            // deduct stock
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.Stock -= item.Quantity;
            }

            // save everything in one transaction
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            _ = _emailService.SendOrderNotificationAsync(
                    order.Id,
                    order.ShippingAddress,
                    order.Total,
                    user?.Email ?? "Unknown"
                );

            // reload with related data for the response
            await _context.Entry(order)
                .Collection(o => o.OrderItems)
                .Query()
                .Include(oi => oi.Product)
                .LoadAsync();

            return OrderResponseDto.FromOrder(order);
        }
    }
}
