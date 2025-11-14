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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
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

                var result = await _categoryService.GetAllAsync(parameters);

                var responseItems = result.Items.Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedDate = c.CreatedDate,
                    ProductCount = c.ProductCount
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
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Category not found"));
                }

                var response = new CategoryResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    CreatedDate = category.CreatedDate,
                    ProductCount = category.ProductCount
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
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create([FromBody] CategoryRequest request)
        {
            try
            {
                var businessModel = new CategoryBusinessModel
                {
                    Name = request.Name,
                    Description = request.Description
                };

                var result = await _categoryService.CreateAsync(businessModel);

                var response = new CategoryResponse
                {
                    CategoryId = result.CategoryId,
                    Name = result.Name,
                    Description = result.Description,
                    CreatedDate = result.CreatedDate,
                    ProductCount = result.ProductCount
                };

                return CreatedAtAction(nameof(GetById), new { id = response.CategoryId },
                    ApiResponse<CategoryResponse>.SuccessResponse(response, "Category created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CategoryResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(int id, [FromBody] CategoryRequest request)
        {
            try
            {
                var businessModel = new CategoryBusinessModel
                {
                    CategoryId = id,
                    Name = request.Name,
                    Description = request.Description
                };

                var result = await _categoryService.UpdateAsync(id, businessModel);

                var response = new CategoryResponse
                {
                    CategoryId = result.CategoryId,
                    Name = result.Name,
                    Description = result.Description,
                    CreatedDate = result.CreatedDate,
                    ProductCount = result.ProductCount
                };

                return Ok(ApiResponse<CategoryResponse>.SuccessResponse(response, "Category updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CategoryResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CategoryResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var result = await _categoryService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Category not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
