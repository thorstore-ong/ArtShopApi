using ArtShopApi.Data;
using ArtShopApi.DTOs.Payment;
using ArtShopApi.Models;
using ArtShopApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ArtShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly YocoService _yocoService;

        public PaymentsController(AppDbContext context, YocoService yocoService)
        {
            _context = context;
            _yocoService = yocoService;
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<ActionResult<InitiatePaymentDto>> InitiatePayment(InitiatePaymentDto dto) 
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);
            if (order == null) 
                return NotFound("Order not Found");

            if (order.Status != "Pending")
                return BadRequest("This order has already been paid or is not payable");

            Console.WriteLine($"Received successUrl: '{dto.SuccessUrl}'");
            Console.WriteLine($"Received cancelUrl: '{dto.CancelUrl}'");

            try
            {
                var paymentUrl = await _yocoService.CreateCheckoutAsync(
                    order.Id,
                    order.Total,
                    dto.SuccessUrl,
                    dto.CancelUrl
                    );

                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.Total,
                    Status = "Pending",
                    YocoChargeId = string.Empty
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return Ok(new PaymentResponseDto { PaymentUrl = paymentUrl });
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message); 
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {


            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            JsonElement payload;
            try
            {
                payload = JsonSerializer.Deserialize<JsonElement>(body);
            }
            catch
            {
                return BadRequest("Invalid JSON.");
            }

            // Extracting the event type
            var eventType = payload.GetProperty("type").GetString();

            if (eventType == "payment.succeeded")
            {
                //Gets order Id from metadata
                var metadata = payload.GetProperty("payload").GetProperty("metadata");
                var orderId = int.Parse(metadata.GetProperty("orderId").GetString()!);
                var yocoChargeId = payload.GetProperty("payload").GetProperty("id").GetString()!;

                // Update order status
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = "Paid";
                    await _context.SaveChangesAsync();
                }

                // Update payment record
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment != null)
                {
                    payment.Status = "Paid";
                    payment.YocoChargeId = yocoChargeId;
                    payment.PaidAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();

        }
    }
}
