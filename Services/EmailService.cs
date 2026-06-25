using Resend;

namespace ArtShopApi.Services
{
    public class EmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _config;

        public EmailService(IResend resend, IConfiguration config)
        {
            _resend = resend;
            _config = config;
        }


        public async Task SendOrderNotificationAsync(int orderId, string shippingAddress, decimal total, string customerEmail)
        {
            var from = _config["Resend:From"];
            var ro = _config["Resend:To"];

            var message = new EmailMessage
            {
                From = from,
                To = { to },
                Subject = $"✦ New Order #{orderId} — Thorstore Art",
                HtmlBody = $"""
                <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2 style="color: #F0278A;">New Order Received! ✦</h2>
                    <p><strong>Order ID:</strong> #{orderId}</p>
                    <p><strong>Customer:</strong> {customerEmail}</p>
                    <p><strong>Shipping to:</strong> {shippingAddress}</p>
                    <p><strong>Total:</strong> R{total:F2}</p>
                    <hr style="border-color: #fce4f0;" />
                    <p style="color: #9a8fa0; font-size: 12px;">Thorstore Art — thorstoreart.co.za</p>
                </div>
            """
            };

            await _resend.EmailSendAsync(message);
        }


        public async Task SendContactEmailAsync(string senderName, string senderEmail, string messageContent)
        {
            var from = _config["Resend:From"]!;
            var to = _config["Resend:To"]!;

            var message = new EmailMessage
            {
                From = from,
                To = { to },
                Subject = $"✦ New message from {senderName} — Thorstore Art",
                HtmlBody = $"""
                <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2 style="color: #F0278A;">New Contact Message ✦</h2>
                    <p><strong>From:</strong> {senderName} ({senderEmail})</p>
                    <hr style="border-color: #fce4f0;" />
                    <p>{messageContent}</p>
                    <hr style="border-color: #fce4f0;" />
                    <p style="color: #9a8fa0; font-size: 12px;">Thorstore Art — thorstoreart.co.za</p>
                </div>
            """
            };

            await _resend.EmailSendAsync(message);
        }
    }
    }
}
