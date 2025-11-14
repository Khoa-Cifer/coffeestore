using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Contracts
{
    public interface IOrderService
    {
        Task<PagedResult<OrderBusinessModel>> GetAllAsync(QueryParameters parameters, string? userId = null);
        Task<OrderBusinessModel?> GetByIdAsync(int id);
        Task<OrderBusinessModel> CreateAsync(OrderBusinessModel model);
        Task<OrderBusinessModel> UpdateAsync(int id, OrderBusinessModel model);
        Task<bool> DeleteAsync(int id);
    }
}
