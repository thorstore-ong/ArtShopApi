using System.Text.Json;
using System.Text;


namespace ArtShopApi.Services
{
    public class YocoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public YocoService(HttpClient httpClient, IConfiguration config)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> CreateCheckoutAsync(int orderId, decimal amount, string successUrl, string cancelUrl)
        {
            // Yoco expects amount in cents, not rands
            var amountInCents = (int)(amount * 100);
            var payload = new
            {
                amount = amountInCents,
                currency = "ZAR",
                successUrl = successUrl,
                cancelUrl = cancelUrl,
                metadata = new { orderId = orderId.ToString() }

            };
            //testing
            Console.WriteLine($"Sending to Yoco - successUrl: '{successUrl}', cancelUrl: '{cancelUrl}'");

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {_config["Yoco:SecretKey"]}"
                );

            var response = await _httpClient.PostAsync(
                "https://payments.yoco.com/api/checkouts", content
                );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Yoco error: {responseBody}");

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var redirectUrl = result.GetProperty("redirectUrl").GetString();

            if (string.IsNullOrEmpty(redirectUrl))
                throw new Exception("Yoco did not return a redirect URL");

            Console.WriteLine($"Full Yoco response: {responseBody}");

            return redirectUrl;
        }
    }
}
