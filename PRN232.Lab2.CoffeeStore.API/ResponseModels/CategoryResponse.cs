namespace PRN232.Lab2.CoffeeStore.API.ResponseModels
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int ProductCount { get; set; }
    }
}
