using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab2.CoffeeStore.API.Helpers;
using PRN232.Lab2.CoffeeStore.API.RequestModels;
using PRN232.Lab2.CoffeeStore.API.ResponseModels;
using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using PRN232.Lab2.CoffeeStore.Services.Contracts;

namespace PRN232.Lab2.CoffeeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json", "application/xml")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
                var parameters = new QueryParameters
                {
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize,
                    Select = select
                };

                var result = await _paymentService.GetAllAsync(parameters);

                var responseItems = result.Items.Select(p => new PaymentResponse
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod
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
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Payment not found"));
                }

                var response = new PaymentResponse
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    PaymentMethod = payment.PaymentMethod
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
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Create([FromBody] PaymentRequest request)
        {
            try
            {
                var businessModel = new PaymentBusinessModel
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod
                };

                var result = await _paymentService.CreateAsync(businessModel);

                var response = new PaymentResponse
                {
                    PaymentId = result.PaymentId,
                    OrderId = result.OrderId,
                    Amount = result.Amount,
                    PaymentDate = result.PaymentDate,
                    PaymentMethod = result.PaymentMethod
                };

                return CreatedAtAction(nameof(GetById), new { id = response.PaymentId },
                    ApiResponse<PaymentResponse>.SuccessResponse(response, "Payment created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Update(int id, [FromBody] PaymentRequest request)
        {
            try
            {
                var businessModel = new PaymentBusinessModel
                {
                    PaymentId = id,
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod
                };

                var result = await _paymentService.UpdateAsync(id, businessModel);

                var response = new PaymentResponse
                {
                    PaymentId = result.PaymentId,
                    OrderId = result.OrderId,
                    Amount = result.Amount,
                    PaymentDate = result.PaymentDate,
                    PaymentMethod = result.PaymentMethod
                };

                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(response, "Payment updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<PaymentResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var result = await _paymentService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Payment not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Payment deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
