namespace PRN232.Lab2.CoffeeStore.API.RequestModels
{
    public class OrderRequest
    {
        public string Status { get; set; } = "Pending";
        public List<OrderDetailRequest> OrderDetails { get; set; } = new();
    }
}
