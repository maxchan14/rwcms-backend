using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using rWCMS.Data;
using rWCMS.Repositories.Interface;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly rWCMSDbContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(rWCMSDbContext context)
        {
            _context = context;
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
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
    }
}