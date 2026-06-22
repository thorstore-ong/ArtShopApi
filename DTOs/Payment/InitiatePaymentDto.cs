namespace ArtShopApi.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        public int OrderId { get; set; }
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl {  get; set; } = string.Empty;
    }
}
