using ArtShopApi.Data;
using ArtShopApi.DTOs.Order;
using ArtShopApi.Models;
using ArtShopApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace ArtShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly AppDbContext _context;

        public OrdersController(OrderService orderService, AppDbContext context)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> PlaceOrder(CreateOrderDto dto)
        {
            //Read's users name from JWT claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            { 
                var order = await _orderService.PlaceOrderAsync(userId, dto);
                return Ok(order);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<OrderResponseDto>>> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (userId == 0)
                return NotFound();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders.Select(OrderResponseDto.FromOrder));
        }

        //Get all orders as admin
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<List<OrderResponseDto>>> GetAllOrders() 
        { 
          var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders.Select(OrderResponseDto.FromOrder));
        
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, [FromBody] string status) 
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var validStatus = new[] { "Pending", "Paid", "Shipped", "Delivered", "Cancelled" };
            if (!validStatus.Contains(status))
                return BadRequest($"Invalid status update. Must be {string.Join(", ", validStatus)}");

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(OrderResponseDto.FromOrder(order));
        }



    }
}
