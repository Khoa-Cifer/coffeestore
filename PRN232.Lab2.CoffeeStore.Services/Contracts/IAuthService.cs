using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthBusinessModel> LoginAsync(string username, string password);
        Task<AuthBusinessModel> RegisterAsync(string username, string email, string password);
        Task<AuthBusinessModel> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
}
