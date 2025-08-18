using rWCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(int userId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<string> GetCurrentUserSidAsync();
        Task<int> GetCurrentUserIdAsync();
        Task<List<int>> GetUserAppGroupIdsAsync(string sid);
        Task<User?> GetUserBySidAsync(string sid);
        Task<bool> IsSiteAdminAsync(string sid);
    }
}