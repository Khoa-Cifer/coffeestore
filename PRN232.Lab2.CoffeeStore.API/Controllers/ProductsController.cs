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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
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
                var parameters = new QueryParameters
                {
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize,
                    Select = select
                };

                var result = await _productService.GetAllAsync(parameters);

                var responseItems = result.Items.Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.CategoryName,
                    IsActive = p.IsActive
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
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Product not found"));
                }

                var response = new ProductResponse
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CategoryName = product.CategoryName,
                    IsActive = product.IsActive
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
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Create([FromBody] ProductRequest request)
        {
            try
            {
                var businessModel = new ProductBusinessModel
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    IsActive = request.IsActive
                };

                var result = await _productService.CreateAsync(businessModel);

                var response = new ProductResponse
                {
                    ProductId = result.ProductId,
                    Name = result.Name,
                    Description = result.Description,
                    Price = result.Price,
                    CategoryId = result.CategoryId,
                    CategoryName = result.CategoryName,
                    IsActive = result.IsActive
                };

                return CreatedAtAction(nameof(GetById), new { id = response.ProductId },
                    ApiResponse<ProductResponse>.SuccessResponse(response, "Product created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ProductResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(int id, [FromBody] ProductRequest request)
        {
            try
            {
                var businessModel = new ProductBusinessModel
                {
                    ProductId = id,
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    IsActive = request.IsActive
                };

                var result = await _productService.UpdateAsync(id, businessModel);

                var response = new ProductResponse
                {
                    ProductId = result.ProductId,
                    Name = result.Name,
                    Description = result.Description,
                    Price = result.Price,
                    CategoryId = result.CategoryId,
                    CategoryName = result.CategoryName,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<ProductResponse>.SuccessResponse(response, "Product updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ProductResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ProductResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Product not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
