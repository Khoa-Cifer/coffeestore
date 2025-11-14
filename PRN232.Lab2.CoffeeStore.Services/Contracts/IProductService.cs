using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Contracts
{
    public interface IProductService
    {
        Task<PagedResult<ProductBusinessModel>> GetAllAsync(QueryParameters parameters);
        Task<ProductBusinessModel?> GetByIdAsync(int id);
        Task<ProductBusinessModel> CreateAsync(ProductBusinessModel model);
        Task<ProductBusinessModel> UpdateAsync(int id, ProductBusinessModel model);
        Task<bool> DeleteAsync(int id);
    }
}
