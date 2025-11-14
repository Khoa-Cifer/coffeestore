using Microsoft.EntityFrameworkCore.Storage;
using PRN232.Lab2.CoffeeStore.Repositories.Contracts;
using PRN232.Lab2.CoffeeStore.Repositories.Data;
using PRN232.Lab2.CoffeeStore.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CoffeeStoreDbContext _context;
        private IDbContextTransaction? _transaction;

        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Product> Products { get; }
        public IGenericRepository<Order> Orders { get; }
        public IGenericRepository<OrderDetail> OrderDetails { get; }
        public IGenericRepository<Payment> Payments { get; }
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }

        public UnitOfWork(CoffeeStoreDbContext context)
        {
            _context = context;
            Categories = new GenericRepository<Category>(_context);
            Products = new GenericRepository<Product>(_context);
            Orders = new GenericRepository<Order>(_context);
            OrderDetails = new GenericRepository<OrderDetail>(_context);
            Payments = new GenericRepository<Payment>(_context);
            Users = new GenericRepository<User>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
