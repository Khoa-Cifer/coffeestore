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
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ProductBusinessModel>> GetAllAsync(QueryParameters parameters)
        {
            Expression<Func<Product, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.ToLower();
                filter = p => p.Name.ToLower().Contains(search) ||
                             p.Description.ToLower().Contains(search);
            }

            Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null;

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                orderBy = parameters.SortBy.ToLower() switch
                {
                    "name" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(p => p.Name)
                        : q.OrderBy(p => p.Name),
                    "price" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(p => p.Price)
                        : q.OrderBy(p => p.Price),
                    _ => q => q.OrderBy(p => p.ProductId)
                };
            }

            var totalCount = await _unitOfWork.Products.CountAsync(filter);

            var products = await _unitOfWork.Products.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                skip: (parameters.Page - 1) * parameters.PageSize,
                take: parameters.PageSize,
                includes: p => p.Category
            );

            var items = products.Select(p => new ProductBusinessModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsActive = p.IsActive
            }).ToList();

            return new PagedResult<ProductBusinessModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<ProductBusinessModel?> GetByIdAsync(int id)
        {
            var products = await _unitOfWork.Products.GetPagedAsync(
                filter: p => p.ProductId == id,
                includes: p => p.Category
            );

            var product = products.FirstOrDefault();
            if (product == null) return null;

            return new ProductBusinessModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                IsActive = product.IsActive
            };
        }

        public async Task<ProductBusinessModel> CreateAsync(ProductBusinessModel model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                CategoryId = model.CategoryId,
                IsActive = model.IsActive
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            model.ProductId = product.ProductId;
            return model;
        }

        public async Task<ProductBusinessModel> UpdateAsync(int id, ProductBusinessModel model)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            await _unitOfWork.Products.DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
