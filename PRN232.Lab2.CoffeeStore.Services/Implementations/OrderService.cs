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
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<OrderBusinessModel>> GetAllAsync(QueryParameters parameters, string? userId = null)
        {
            Expression<Func<Order, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                filter = o => o.UserId == userId;
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search) && filter != null)
            {
                var search = parameters.Search.ToLower();
                var existingFilter = filter;
                filter = o => existingFilter.Compile()(o) && o.Status.ToLower().Contains(search);
            }
            else if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.ToLower();
                filter = o => o.Status.ToLower().Contains(search);
            }

            Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = null;

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                orderBy = parameters.SortBy.ToLower() switch
                {
                    "orderdate" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(o => o.OrderDate)
                        : q.OrderBy(o => o.OrderDate),
                    "status" => q => parameters.SortOrder.ToLower() == "desc"
                        ? q.OrderByDescending(o => o.Status)
                        : q.OrderBy(o => o.Status),
                    _ => q => q.OrderByDescending(o => o.OrderDate)
                };
            }

            var totalCount = await _unitOfWork.Orders.CountAsync(filter);

            var orders = await _unitOfWork.Orders.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                skip: (parameters.Page - 1) * parameters.PageSize,
                take: parameters.PageSize,
                includes: o => o.OrderDetails
            );

            var items = new List<OrderBusinessModel>();
            foreach (var order in orders)
            {
                var orderDetails = await _unitOfWork.OrderDetails.GetPagedAsync(
                    filter: od => od.OrderId == order.OrderId,
                    includes: od => od.Product
                );

                items.Add(new OrderBusinessModel
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    PaymentId = order.PaymentId,
                    TotalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice),
                    OrderDetails = orderDetails.Select(od => new OrderDetailBusinessModel
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.Name ?? "",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.Quantity * od.UnitPrice
                    }).ToList()
                });
            }

            return new PagedResult<OrderBusinessModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<OrderBusinessModel?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return null;

            var orderDetails = await _unitOfWork.OrderDetails.GetPagedAsync(
                filter: od => od.OrderId == id,
                includes: od => od.Product
            );

            return new OrderBusinessModel
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentId = order.PaymentId,
                TotalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice),
                OrderDetails = orderDetails.Select(od => new OrderDetailBusinessModel
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name ?? "",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.Quantity * od.UnitPrice
                }).ToList()
            };
        }

        public async Task<OrderBusinessModel> CreateAsync(OrderBusinessModel model)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = model.UserId,
                    OrderDate = DateTime.UtcNow,
                    Status = model.Status
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                foreach (var detail in model.OrderDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice
                    };

                    await _unitOfWork.OrderDetails.AddAsync(orderDetail);
                }

                await _unitOfWork.CommitTransactionAsync();

                model.OrderId = order.OrderId;
                model.OrderDate = order.OrderDate;
                return model;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderBusinessModel> UpdateAsync(int id, OrderBusinessModel model)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found");
            }

            order.Status = model.Status;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return false;

            await _unitOfWork.Orders.DeleteAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
