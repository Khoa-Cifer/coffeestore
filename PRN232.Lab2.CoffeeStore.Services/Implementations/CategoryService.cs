using PRN232.Lab2.CoffeeStore.Repositories.Contracts;
using PRN232.Lab2.CoffeeStore.Repositories.Models;
using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using PRN232.Lab2.CoffeeStore.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CategoryBusinessModel>> GetAllAsync(QueryParameters parameters)
        {
            Expression<Func<Category, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.ToLower();
                filter = c => c.Name.ToLower().Contains(search) ||
                             c.Description.ToLower().Contains(search);
            }

            Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null;

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                orderBy = parameters.SortBy.ToLower() switch
                {
                    "name" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(c => c.Name)
                        : q.OrderBy(c => c.Name),
                    "createddate" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(c => c.CreatedDate)
                        : q.OrderBy(c => c.CreatedDate),
                    _ => q => q.OrderBy(c => c.CategoryId)
                };
            }

            var totalCount = await _unitOfWork.Categories.CountAsync(filter);

            var categories = await _unitOfWork.Categories.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                skip: (parameters.Page - 1) * parameters.PageSize,
                take: parameters.PageSize,
                includes: c => c.Products
            );

            var items = categories.Select(c => new CategoryBusinessModel
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                CreatedDate = c.CreatedDate,
                ProductCount = c.Products.Count
            }).ToList();

            return new PagedResult<CategoryBusinessModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<CategoryBusinessModel?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return null;

            var products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == id);

            return new CategoryBusinessModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                CreatedDate = category.CreatedDate,
                ProductCount = products.Count()
            };
        }

        public async Task<CategoryBusinessModel> CreateAsync(CategoryBusinessModel model)
        {
            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            model.CategoryId = category.CategoryId;
            model.CreatedDate = category.CreatedDate;
            return model;
        }

        public async Task<CategoryBusinessModel> UpdateAsync(int id, CategoryBusinessModel model)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            category.Name = model.Name;
            category.Description = model.Description;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return false;

            await _unitOfWork.Categories.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
