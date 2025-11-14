namespace PRN232.Lab2.CoffeeStore.API.RequestModels
{
    public class ProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
