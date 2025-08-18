using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using rWCMS.Services.Interface;
using rWCMS.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;

        public PermissionService(IPermissionRepository permissionRepository, IUserRepository userRepository)
        {
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
        }

        public async Task<List<PathPermission>> GetPathPermissionsAsync(string path, bool includeInherited)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var appGroupIds = await _permissionRepository.GetUserAppGroupIdsAsync(sid);
            if (!await _permissionRepository.HasPermissionAsync(appGroupIds, path, BasePermissionType.Read, sid))
                throw new UnauthorizedAccessException($"Permission {BasePermissionType.Read} denied for path {path}");
            return await _permissionRepository.GetPathPermissionsAsync(path, includeInherited);
        }

        public async Task<List<PathPermission>> GetEffectivePermissionsAsync(string path)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            return await _permissionRepository.GetEffectivePermissionsAsync(path, sid);
        }

        public async Task BreakInheritanceAsync(string path)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var appGroupIds = await _permissionRepository.GetUserAppGroupIdsAsync(sid);
            if (!await _permissionRepository.HasPermissionAsync(appGroupIds, path, BasePermissionType.ManagePathPermissions, sid))
                throw new UnauthorizedAccessException($"Permission {BasePermissionType.ManagePathPermissions} denied for path {path}");
            var userId = await _userRepository.GetCurrentUserIdAsync();
            await _permissionRepository.BreakInheritanceAsync(path, userId);
            await _permissionRepository.InvalidateCachesAsync(userId);
        }

        public async Task RevertToInheritanceAsync(string path)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var appGroupIds = await _permissionRepository.GetUserAppGroupIdsAsync(sid);
            if (!await _permissionRepository.HasPermissionAsync(appGroupIds, path, BasePermissionType.ManagePathPermissions, sid))
                throw new UnauthorizedAccessException($"Permission {BasePermissionType.ManagePathPermissions} denied for path {path}");
            var userId = await _userRepository.GetCurrentUserIdAsync();
            await _permissionRepository.RevertToInheritanceAsync(path, userId);
            await _permissionRepository.InvalidateCachesAsync(userId);
        }

        public async Task GrantPermissionAsync(GrantPermissionRequest request)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var appGroupIds = await _permissionRepository.GetUserAppGroupIdsAsync(sid);
            if (!await _permissionRepository.HasPermissionAsync(appGroupIds, request.Path, BasePermissionType.ManagePathPermissions, sid))
                throw new UnauthorizedAccessException($"Permission {BasePermissionType.ManagePathPermissions} denied for path {request.Path}");
            var userId = await _userRepository.GetCurrentUserIdAsync();
            var permission = await _permissionRepository.GetPathPermissionByPathAsync(request.Path);
            if (permission == null)
            {
                permission = new PathPermission
                {
                    Path = request.Path,
                    CreatedById = userId,
                    CreatedDate = DateTime.UtcNow
                };
                await _permissionRepository.AddPathPermissionAsync(permission);
            }

            var assignment = new PathPermissionAssignment
            {
                PathPermissionId = permission.PathPermissionId,
                AppGroupId = request.AppGroupId,
                PermissionLevelId = request.PermissionLevelId,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow,
                ModifiedById = userId,
                ModifiedDate = DateTime.UtcNow
            };
            await _permissionRepository.AddPathPermissionAssignmentAsync(assignment);
            await _permissionRepository.InvalidateCachesAsync(userId);
        }
    }
}