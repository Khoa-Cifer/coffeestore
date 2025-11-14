using PRN232.Lab2.CoffeeStore.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Repositories.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
