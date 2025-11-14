using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Contracts
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryBusinessModel>> GetAllAsync(QueryParameters parameters);
        Task<CategoryBusinessModel?> GetByIdAsync(int id);
        Task<CategoryBusinessModel> CreateAsync(CategoryBusinessModel model);
        Task<CategoryBusinessModel> UpdateAsync(int id, CategoryBusinessModel model);
        Task<bool> DeleteAsync(int id);
    }
}
