namespace PRN232.Lab2.CoffeeStore.API.RequestModels
{
    public class AuthRegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
