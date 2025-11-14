using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab2.CoffeeStore.API.RequestModels;
using PRN232.Lab2.CoffeeStore.API.ResponseModels;
using PRN232.Lab2.CoffeeStore.Services.Contracts;

namespace PRN232.Lab2.CoffeeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/xml")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] AuthLoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Username, request.Password);

                var response = new AuthResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    User = new UserResponse
                    {
                        UserId = result.User.UserId,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        Role = result.User.Role
                    }
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] AuthRegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);

                var response = new AuthResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    User = new UserResponse
                    {
                        UserId = result.User.UserId,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        Role = result.User.Role
                    }
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                var response = new AuthResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    User = new UserResponse
                    {
                        UserId = result.User.UserId,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        Role = result.User.Role
                    }
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                await _authService.LogoutAsync(request.RefreshToken);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
