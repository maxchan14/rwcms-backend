using rWCMS.Models;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IAuditLogRepository
    {
        Task AddAuditLogAsync(AuditLog auditLog);
    }
}