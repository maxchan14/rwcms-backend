using rWCMS.Enum;
using rWCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IPermissionRepository
    {
        Task<List<PathPermission>> GetPathPermissionsAsync(string path, bool includeInherited);
        Task<List<PathPermission>> GetEffectivePermissionsAsync(string path, string sid);
        Task BreakInheritanceAsync(string path, int userId);
        Task RevertToInheritanceAsync(string path, int userId);
        Task AddPathPermissionAsync(PathPermission permission);
        Task AddPathPermissionAssignmentAsync(PathPermissionAssignment assignment);
        Task<PathPermission?> GetPathPermissionByPathAsync(string path);
        Task RequirePermissionAsync(string path, BasePermissionType permissionType);
        Task<bool> HasPermissionAsync(string path, BasePermissionType permissionType);
        Task<bool> HasPermissionAsync(List<int> userAppGroupIds, string path, BasePermissionType permissionType, string sid);
        Task<List<int>> GetUserAppGroupIdsAsync(string sid);
        Task<List<PathPermission>> GetAllPathPermissionsAsync();
        Task InvalidateCachesAsync(int userId);
        Task UpdatePathPermissionsAsync(string oldPath, string newPath);
        Task<List<PathPermission>> GetUserPathPermissionsAsync(string sid);
        Task<bool> HaveSharedBundleAsync(List<int> groupIds1, List<int> groupIds2);
    }
}