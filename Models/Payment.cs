namespace ArtShopApi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string YocoChargeId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
