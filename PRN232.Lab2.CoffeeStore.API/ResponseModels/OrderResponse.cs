namespace PRN232.Lab2.CoffeeStore.API.ResponseModels
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; } = new();
    }
}
