using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab2.CoffeeStore.API.Helpers;
using PRN232.Lab2.CoffeeStore.API.RequestModels;
using PRN232.Lab2.CoffeeStore.API.ResponseModels;
using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using PRN232.Lab2.CoffeeStore.Services.Contracts;
using System.Security.Claims;

namespace PRN232.Lab2.CoffeeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json", "application/xml")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? select = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var parameters = new QueryParameters
                {
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize,
                    Select = select
                };

                // Non-admin users can only see their own orders
                var result = await _orderService.GetAllAsync(parameters,
                    userRole != "Admin" ? userId : null);

                var responseItems = result.Items.Select(o => new OrderResponse
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.TotalPrice
                    }).ToList()
                }).ToList();

                var data = string.IsNullOrWhiteSpace(select)
                    ? (object)new { result.TotalCount, result.PageNumber, result.PageSize, result.TotalPages, Items = responseItems }
                    : new { result.TotalCount, result.PageNumber, result.PageSize, result.TotalPages, Items = FieldSelector.SelectFields(responseItems, select) };

                return Ok(ApiResponse<object>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetById(int id, [FromQuery] string? select = null)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Order not found"));
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Check authorization
                if (userRole != "Admin" && order.UserId != userId)
                {
                    return Forbid();
                }

                var response = new OrderResponse
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.TotalPrice
                    }).ToList()
                };

                var data = string.IsNullOrWhiteSpace(select)
                    ? (object)response
                    : FieldSelector.SelectFields(response, select);

                return Ok(ApiResponse<object>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> Create([FromBody] OrderRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OrderResponse>.ErrorResponse("User not authenticated"));
                }

                var businessModel = new OrderBusinessModel
                {
                    UserId = userId,
                    Status = request.Status,
                    OrderDetails = request.OrderDetails.Select(od => new OrderDetailBusinessModel
                    {
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                };

                var result = await _orderService.CreateAsync(businessModel);

                var response = new OrderResponse
                {
                    OrderId = result.OrderId,
                    UserId = result.UserId,
                    OrderDate = result.OrderDate,
                    Status = result.Status,
                    TotalAmount = result.TotalAmount,
                    OrderDetails = result.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.TotalPrice
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetById), new { id = response.OrderId },
                    ApiResponse<OrderResponse>.SuccessResponse(response, "Order created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OrderResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> Update(int id, [FromBody] OrderRequest request)
        {
            try
            {
                var existingOrder = await _orderService.GetByIdAsync(id);
                if (existingOrder == null)
                {
                    return NotFound(ApiResponse<OrderResponse>.ErrorResponse("Order not found"));
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Check authorization
                if (userRole != "Admin" && existingOrder.UserId != userId)
                {
                    return Forbid();
                }

                var businessModel = new OrderBusinessModel
                {
                    OrderId = id,
                    UserId = existingOrder.UserId,
                    Status = request.Status
                };

                var result = await _orderService.UpdateAsync(id, businessModel);

                var response = new OrderResponse
                {
                    OrderId = result.OrderId,
                    UserId = result.UserId,
                    OrderDate = result.OrderDate,
                    Status = result.Status,
                    TotalAmount = result.TotalAmount
                };

                return Ok(ApiResponse<OrderResponse>.SuccessResponse(response, "Order updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OrderResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Order not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Order deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
