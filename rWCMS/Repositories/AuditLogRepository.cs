using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly rWCMSDbContext _context;

        public AuditLogRepository(rWCMSDbContext context)
        {
            _context = context;
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}