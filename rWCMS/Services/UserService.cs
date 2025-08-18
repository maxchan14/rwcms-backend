using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Repositories.Interface;
using rWCMS.Services.Interface;
using rWCMS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rWCMS.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;

        public UserService(IUserRepository userRepository, IPermissionRepository permissionRepository)
        {
            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var user = await _userRepository.GetUserAsync(await _userRepository.GetCurrentUserIdAsync());
            return new UserDto
            {
                UserId = user.UserId,
                SID = user.SID,
                AdLoginId = user.AdLoginId,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                IsSiteAdmin = user.IsSiteAdmin
            };
        }

        public async Task<IEnumerable<PathPermissionDto>> GetCurrentUserPermissionsAsync()
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var isSiteAdmin = await _userRepository.IsSiteAdminAsync(sid);
            if (isSiteAdmin)
            {
                return new List<PathPermissionDto>
                {
                    new PathPermissionDto
                    {
                        Path = "*",
                        Permissions = System.Enum.GetNames(typeof(BasePermissionType)).ToList()
                    }
                };
            }

            var pathPermissions = await _permissionRepository.GetUserPathPermissionsAsync(sid);

            var permissionsList = new List<PathPermissionDto>();

            foreach (var pathPerm in pathPermissions)
            {
                var perms = new HashSet<string>();
                foreach (var assignment in pathPerm.PathPermissionAssignments)
                {
                    foreach (var pla in assignment.PermissionLevel.PermissionLevelAssignments)
                    {
                        perms.Add(pla.BasePermission.Name);
                    }
                }

                if (perms.Any())
                {
                    permissionsList.Add(new PathPermissionDto
                    {
                        Path = pathPerm.Path,
                        Permissions = perms.ToList()
                    });
                }
            }

            return permissionsList;
        }
    }
}