using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using rWCMS.Utilities;

namespace rWCMS.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly rWCMSDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ActiveDirectoryUtility _adUtility;
        private const string UserSessionKey = "CurrentUser";

        public UserRepository(
            rWCMSDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ActiveDirectoryUtility adUtility)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _adUtility = adUtility;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User not found: User with ID {userId}");
            return user;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await GetUserAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<User> GetOrSetUserInSessionAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                throw new Exception("Session not available");
            }

            // Try to get user from session
            if (session.TryGetValue(UserSessionKey, out var userBytes))
            {
                var user = JsonSerializer.Deserialize<User>(userBytes);
                if (user != null)
                {
                    return user;
                }
            }

            // If not in session, get from claims and database
            var userFromClaims = await GetUserFromClaimsAsync();
            if (userFromClaims == null)
            {
                throw new Exception("User not found");
            }

            // Store in session
            session.SetString(UserSessionKey, JsonSerializer.Serialize(userFromClaims));
            return userFromClaims;
        }

        private async Task<User> GetUserFromClaimsAsync()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            if (userClaims == null || !userClaims.Identity.IsAuthenticated)
            {
                throw new Exception("User not authenticated");
            }

            var primarySidClaim = userClaims.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid") ??
                                 userClaims.FindFirst(ClaimTypes.NameIdentifier) ??
                                 userClaims.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier") ??
                                 userClaims.FindFirst(ClaimTypes.Sid);

            if (primarySidClaim == null)
            {
                var claims = string.Join(", ", userClaims.Claims.Select(c => $"{c.Type}: {c.Value}"));
                throw new Exception("User primary SID not found in claims");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.SID == primarySidClaim.Value);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return user;
        }

        public async Task<string> GetCurrentUserSidAsync()
        {
            var user = await GetOrSetUserInSessionAsync();
            return user.SID;
        }

        public async Task<int> GetCurrentUserIdAsync()
        {
            var user = await GetOrSetUserInSessionAsync();
            return user.UserId;
        }

        public async Task<List<int>> GetUserAppGroupIdsAsync(string sid)
        {
            // Retrieve all SIDs (user and group SIDs) from Active Directory
            var sids = _adUtility.GetUserGroupSids(sid);

            // Retrieve ADEntityIds from ADEntities table using all SIDs
            var adEntityIds = await _context.ADEntities
                .Where(a => sids.Contains(a.SID))
                .Select(a => a.ADEntityId)
                .ToListAsync();

            // Retrieve AppGroupIds from AppGroupMembers using ADEntityIds
            var appGroupIds = await _context.AppGroupMembers
                .Where(agm => adEntityIds.Contains(agm.ADEntityId))
                .Select(agm => agm.AppGroupId)
                .Distinct()
                .ToListAsync();

            return appGroupIds;
        }

        public async Task<User?> GetUserBySidAsync(string sid)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SID == sid);
        }

        public async Task<bool> IsSiteAdminAsync(string sid)
        {
            return await _context.Users
                .Where(u => u.SID == sid)
                .Select(u => u.IsSiteAdmin)
                .FirstOrDefaultAsync();
        }
    }
}