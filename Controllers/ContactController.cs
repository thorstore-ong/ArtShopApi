using ArtShopApi.Services;
using Microsoft.AspNetCore.Mvc;
using ArtShopApi.DTOs;
using Resend;

namespace ArtShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly EmailService _emailService;

        public ContactController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("All fields are required.");

            await _emailService.SendContactEmailAsync(dto.Name, dto.Email, dto.Message);
            return Ok(new { message = "Message sent successfully." });
        }
    }
}
