using PRN232.Lab2.CoffeeStore.Services.BusinessModels;
using PRN232.Lab2.CoffeeStore.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Contracts
{
    public interface IPaymentService
    {
        Task<PagedResult<PaymentBusinessModel>> GetAllAsync(QueryParameters parameters);
        Task<PaymentBusinessModel?> GetByIdAsync(int id);
        Task<PaymentBusinessModel> CreateAsync(PaymentBusinessModel model);
        Task<PaymentBusinessModel> UpdateAsync(int id, PaymentBusinessModel model);
        Task<bool> DeleteAsync(int id);
    }
}
