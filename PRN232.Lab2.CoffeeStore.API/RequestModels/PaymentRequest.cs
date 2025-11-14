namespace PRN232.Lab2.CoffeeStore.API.RequestModels
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
