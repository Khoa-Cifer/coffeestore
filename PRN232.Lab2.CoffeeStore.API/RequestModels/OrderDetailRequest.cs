namespace PRN232.Lab2.CoffeeStore.API.RequestModels
{
    public class OrderDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
