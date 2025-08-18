using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Services.Interface
{
    public interface IPermissionService
    {
        Task<List<PathPermission>> GetPathPermissionsAsync(string path, bool includeInherited);
        Task<List<PathPermission>> GetEffectivePermissionsAsync(string path);
        Task BreakInheritanceAsync(string path);
        Task RevertToInheritanceAsync(string path);
        Task GrantPermissionAsync(GrantPermissionRequest request);
    }
}