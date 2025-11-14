using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.Lab2.CoffeeStore.Repositories.Contracts;
using PRN232.Lab2.CoffeeStore.Repositories.Models;
using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthBusinessModel> LoginAsync(string username, string password)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthBusinessModel> RegisterAsync(string username, string email, string password)
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == username || u.Email == email);
            if (existingUsers.Any())
            {
                throw new InvalidOperationException("Username or email already exists");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = "User",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthBusinessModel> RefreshTokenAsync(string refreshToken)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt =>
                rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            var token = tokens.FirstOrDefault();
            if (token == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(token.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Revoke old token
            token.IsRevoked = true;
            await _unitOfWork.RefreshTokens.UpdateAsync(token);

            return await GenerateAuthResponse(user);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == refreshToken);
            var token = tokens.FirstOrDefault();

            if (token != null)
            {
                token.IsRevoked = true;
                await _unitOfWork.RefreshTokens.UpdateAsync(token);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "YourSecretKeyHere123456789012345678901234567890");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private async Task<AuthBusinessModel> GenerateAuthResponse(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthBusinessModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserBusinessModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedDate = user.CreatedDate
                }
            };
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "YourSecretKeyHere123456789012345678901234567890");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
