using rWCMS.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Services.Interface
{
    public interface IUserService
    {
        Task<UserDto> GetCurrentUserAsync();
        Task<IEnumerable<PathPermissionDto>> GetCurrentUserPermissionsAsync();
    }
}