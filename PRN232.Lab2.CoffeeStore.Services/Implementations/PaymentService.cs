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
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<PaymentBusinessModel>> GetAllAsync(QueryParameters parameters)
        {
            Expression<Func<Payment, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.ToLower();
                filter = p => p.PaymentMethod.ToLower().Contains(search);
            }

            Func<IQueryable<Payment>, IOrderedQueryable<Payment>>? orderBy = null;

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                orderBy = parameters.SortBy.ToLower() switch
                {
                    "paymentdate" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(p => p.PaymentDate)
                        : q.OrderBy(p => p.PaymentDate),
                    "amount" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(p => p.Amount)
                        : q.OrderBy(p => p.Amount),
                    _ => q => q.OrderByDescending(p => p.PaymentDate)
                };
            }

            var totalCount = await _unitOfWork.Payments.CountAsync(filter);

            var payments = await _unitOfWork.Payments.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                skip: (parameters.Page - 1) * parameters.PageSize,
                take: parameters.PageSize
            );

            var items = payments.Select(p => new PaymentBusinessModel
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod
            }).ToList();

            return new PagedResult<PaymentBusinessModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<PaymentBusinessModel?> GetByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null) return null;

            return new PaymentBusinessModel
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod
            };
        }

        public async Task<PaymentBusinessModel> CreateAsync(PaymentBusinessModel model)
        {
            var payment = new Payment
            {
                OrderId = model.OrderId,
                Amount = model.Amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = model.PaymentMethod
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Update order with payment ID
            var order = await _unitOfWork.Orders.GetByIdAsync(model.OrderId);
            if (order != null)
            {
                order.PaymentId = payment.PaymentId;
                order.Status = "Paid";
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
            }

            model.PaymentId = payment.PaymentId;
            model.PaymentDate = payment.PaymentDate;
            return model;
        }

        public async Task<PaymentBusinessModel> UpdateAsync(int id, PaymentBusinessModel model)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment with ID {id} not found");
            }

            payment.Amount = model.Amount;
            payment.PaymentMethod = model.PaymentMethod;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null) return false;

            await _unitOfWork.Payments.DeleteAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
