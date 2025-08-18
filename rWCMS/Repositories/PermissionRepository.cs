using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Enum;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rWCMS.Utilities;
using System;
using System.Collections.Concurrent;

namespace rWCMS.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly rWCMSDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;

        private static readonly ConcurrentDictionary<string, (List<PathPermission> Permissions, DateTime LastUpdated)> _permissionCache = new();
        private static readonly ConcurrentDictionary<string, (List<int> AppGroupIds, DateTime LastUpdated)> _userAppGroupCache = new();

        public PermissionRepository(rWCMSDbContext context, IUserRepository userRepository, ISystemSettingRepository systemSettingRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _systemSettingRepository = systemSettingRepository;
        }

        public async Task<List<PathPermission>> GetPathPermissionsAsync(string path, bool includeInherited)
        {
            var allPermissions = await GetAllPathPermissionsAsync();

            if (!includeInherited || path.StartsWith("#"))
            {
                return allPermissions.Where(p => p.Path == path).ToList();
            }
            else
            {
                return allPermissions.Where(p => PathMatcher.AppliesTo(path, p.Path)).ToList();
            }
        }

        public async Task<List<PathPermission>> GetEffectivePermissionsAsync(string path, string sid)
        {
            var userAppGroupIds = await GetUserAppGroupIdsAsync(sid);
            var allPermissions = await GetAllPathPermissionsAsync();
            var isGlobal = path.StartsWith("#");

            var filteredPermissions = allPermissions
                .Where(p => PathMatcher.AppliesTo(path, p.Path, isGlobal))
                .Where(p => p.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId)))
                .Select(p => new PathPermission
                {
                    PathPermissionId = p.PathPermissionId,
                    Path = p.Path,
                    CreatedDate = p.CreatedDate,
                    CreatedById = p.CreatedById,
                    PathPermissionAssignments = p.PathPermissionAssignments
                        .Where(ppa => userAppGroupIds.Contains(ppa.AppGroupId))
                        .Select(ppa => new PathPermissionAssignment
                        {
                            PathPermissionId = ppa.PathPermissionId,
                            AppGroupId = ppa.AppGroupId,
                            PermissionLevelId = ppa.PermissionLevelId,
                            CreatedDate = ppa.CreatedDate,
                            CreatedById = ppa.CreatedById,
                            ModifiedDate = ppa.ModifiedDate,
                            ModifiedById = ppa.ModifiedById,
                            PermissionLevel = new PermissionLevel
                            {
                                PermissionLevelId = ppa.PermissionLevel.PermissionLevelId,
                                Name = ppa.PermissionLevel.Name,
                                Description = ppa.PermissionLevel.Description,
                                CreatedDate = ppa.PermissionLevel.CreatedDate,
                                CreatedById = ppa.PermissionLevel.CreatedById,
                                ModifiedDate = ppa.PermissionLevel.ModifiedDate,
                                ModifiedById = ppa.PermissionLevel.ModifiedById,
                                RowVersion = ppa.PermissionLevel.RowVersion,
                                PermissionLevelAssignments = ppa.PermissionLevel.PermissionLevelAssignments.Select(pla => new PermissionLevelAssignment
                                {
                                    PermissionLevelId = pla.PermissionLevelId,
                                    BasePermissionId = pla.BasePermissionId,
                                    BasePermission = new BasePermission
                                    {
                                        BasePermissionId = pla.BasePermission.BasePermissionId,
                                        Name = pla.BasePermission.Name,
                                        Description = pla.BasePermission.Description
                                    }
                                }).ToList()
                            }
                        }).ToList()
                })
                .ToList();

            return filteredPermissions;
        }

        public async Task BreakInheritanceAsync(string path, int userId)
        {
            if (path.StartsWith("#")) return; // No inheritance for global paths

            var permissions = await GetPathPermissionsAsync(path, false);
            if (!permissions.Any())
            {
                var permission = new PathPermission
                {
                    Path = path,
                    CreatedById = userId,
                    CreatedDate = DateTime.UtcNow
                };
                await _context.PathPermissions.AddAsync(permission);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RevertToInheritanceAsync(string path, int userId)
        {
            if (path.StartsWith("#")) return; // No inheritance for global paths

            var permissions = await GetPathPermissionsAsync(path, false);
            _context.PathPermissions.RemoveRange(permissions);
            await _context.SaveChangesAsync();
        }

        public async Task AddPathPermissionAsync(PathPermission permission)
        {
            await _context.PathPermissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task AddPathPermissionAssignmentAsync(PathPermissionAssignment assignment)
        {
            await _context.PathPermissionAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task<PathPermission?> GetPathPermissionByPathAsync(string path)
        {
            return await _context.PathPermissions
                .FirstOrDefaultAsync(p => p.Path == path);
        }

        public async Task RequirePermissionAsync(string path, BasePermissionType permissionType)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var isSiteAdmin = await _userRepository.IsSiteAdminAsync(sid);
            if (isSiteAdmin) return;

            var userAppGroupIds = await GetUserAppGroupIdsAsync(sid);
            if (!await HasPermissionAsync(userAppGroupIds, path, permissionType, sid))
            {
                throw new UnauthorizedAccessException($"Permission {permissionType} denied for path {path}");
            }
        }

        public async Task<bool> HasPermissionAsync(string path, BasePermissionType permissionType)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var isSiteAdmin = await _userRepository.IsSiteAdminAsync(sid);
            if (isSiteAdmin) return true;

            var userAppGroupIds = await GetUserAppGroupIdsAsync(sid);
            return await HasPermissionAsync(userAppGroupIds, path, permissionType, sid);
        }

        public async Task<bool> HasPermissionAsync(List<int> userAppGroupIds, string path, BasePermissionType permissionType, string sid)
        {
            if (await _userRepository.IsSiteAdminAsync(sid))
                return true;

            var permissions = await GetAllPathPermissionsAsync();
            var isGlobal = path.StartsWith("#");

            var matchingPermissions = permissions.Where(p => PathMatcher.AppliesTo(path, p.Path, isGlobal)).ToList();

            var ordered = matchingPermissions.Select(p =>
            {
                int matchLength = GetMatchLength(path, p.Path, isGlobal);
                return new { Perm = p, Length = matchLength };
            }).Where(x => x.Length > 0).OrderByDescending(x => x.Length).ToList();

            if (!ordered.Any()) return false;

            var mostSpecific = ordered.First().Perm;

            bool hasExact = mostSpecific.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId) &&
                ppa.PermissionLevel.PermissionLevelAssignments.Any(pla => pla.BasePermission.Name == permissionType.ToString()));

            if (hasExact) return true;

            if (isGlobal)
            {
                string permStr = permissionType.ToString();

                bool HasHigher(string baseSuffix, params string[] higherSuffixes)
                {
                    return mostSpecific.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId) &&
                        ppa.PermissionLevel.PermissionLevelAssignments.Any(pla =>
                            higherSuffixes.Any(h => pla.BasePermission.Name == permStr.Replace(baseSuffix, h))));
                }

                if (permStr.EndsWith("OwnWorkflows"))
                {
                    if (HasHigher("Own", "Group", "All")) return true;
                }

                if (permStr.EndsWith("GroupWorkflows"))
                {
                    if (HasHigher("Group", "All")) return true;
                }

                if (permStr.EndsWith("ViewAllWorkflows"))
                {
                    return false; // No higher for ViewAll
                }

                if (permStr.EndsWith("OwnWorkflows".Replace("View", "Update")))
                {
                    if (HasHigher("Own", "Group", "All")) return true;
                }

                if (permStr.EndsWith("GroupWorkflows".Replace("View", "Update")))
                {
                    if (HasHigher("Group", "All")) return true;
                }

                if (permStr.EndsWith("OwnWorkflows".Replace("View", "Approve")))
                {
                    if (HasHigher("Own", "Group", "All")) return true;
                }

                if (permStr.EndsWith("GroupWorkflows".Replace("View", "Approve")))
                {
                    if (HasHigher("Group", "All")) return true;
                }

                if (permStr == "UnlockGroupFiles")
                {
                    if (mostSpecific.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId) &&
                        ppa.PermissionLevel.PermissionLevelAssignments.Any(pla => pla.BasePermission.Name == "UnlockAllFiles")))
                        return true;
                }

                if (permStr == "ManageGroup")
                {
                    if (mostSpecific.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId) &&
                        ppa.PermissionLevel.PermissionLevelAssignments.Any(pla => pla.BasePermission.Name == "ManageAllGroups")))
                        return true;
                }
            }

            return false;
        }

        public async Task<List<PathPermission>> GetAllPathPermissionsAsync()
        {
            var lastUpdate = await _systemSettingRepository.GetLatestSystemPermissionUpdateAsync() ?? DateTime.MinValue;
            const string cacheKey = "GlobalPermissions";

            if (_permissionCache.TryGetValue(cacheKey, out var cached) && cached.LastUpdated >= lastUpdate)
            {
                return cached.Permissions;
            }

            var permissions = await _context.PathPermissions
                .Include(p => p.PathPermissionAssignments)
                    .ThenInclude(ppa => ppa.PermissionLevel)
                        .ThenInclude(pl => pl.PermissionLevelAssignments)
                            .ThenInclude(pla => pla.BasePermission)
                .ToListAsync();

            _permissionCache[cacheKey] = (permissions, DateTime.UtcNow);
            return permissions;
        }

        public async Task<List<int>> GetUserAppGroupIdsAsync(string sid)
        {
            var lastUpdate = await _systemSettingRepository.GetLatestSystemPermissionUpdateAsync() ?? DateTime.MinValue;
            var cacheKey = $"UserAppGroups_{sid}";

            if (_userAppGroupCache.TryGetValue(cacheKey, out var cached) && cached.LastUpdated >= lastUpdate)
            {
                return cached.AppGroupIds;
            }

            var userAppGroupIds = await _userRepository.GetUserAppGroupIdsAsync(sid);
            _userAppGroupCache[cacheKey] = (userAppGroupIds, DateTime.UtcNow);
            return userAppGroupIds;
        }

        public async Task InvalidateCachesAsync(int userId)
        {
            var systemSetting = await _systemSettingRepository.GetSettingAsync(1); // Assuming SettingId 1
            if (systemSetting == null)
            {
                systemSetting = new SystemSetting
                {
                    SystemPermissionUpdate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    LastUpdatedById = userId
                };
                await _systemSettingRepository.AddSettingAsync(systemSetting);
            }
            else
            {
                systemSetting.SystemPermissionUpdate = DateTime.UtcNow;
                systemSetting.LastUpdatedDate = DateTime.UtcNow;
                systemSetting.LastUpdatedById = userId;
                await _systemSettingRepository.UpdateSettingAsync(systemSetting);
            }

            _permissionCache.Clear();
            _userAppGroupCache.Clear();
        }

        public async Task<List<PathPermission>> GetUserPathPermissionsAsync(string sid)
        {
            var lastUpdate = await _systemSettingRepository.GetLatestSystemPermissionUpdateAsync() ?? DateTime.MinValue;
            var cacheKey = $"UserPathPermissions_{sid}";

            if (_permissionCache.TryGetValue(cacheKey, out var cached) && cached.LastUpdated >= lastUpdate)
            {
                return cached.Permissions;
            }

            var userAppGroupIds = await GetUserAppGroupIdsAsync(sid);
            var allPermissions = await GetAllPathPermissionsAsync();

            var userPermissions = allPermissions
                .Where(p => p.PathPermissionAssignments.Any(ppa => userAppGroupIds.Contains(ppa.AppGroupId)))
                .Select(p => new PathPermission
                {
                    PathPermissionId = p.PathPermissionId,
                    Path = p.Path,
                    CreatedDate = p.CreatedDate,
                    CreatedById = p.CreatedById,
                    PathPermissionAssignments = p.PathPermissionAssignments
                        .Where(ppa => userAppGroupIds.Contains(ppa.AppGroupId))
                        .Select(ppa => new PathPermissionAssignment
                        {
                            PathPermissionId = ppa.PathPermissionId,
                            AppGroupId = ppa.AppGroupId,
                            PermissionLevelId = ppa.PermissionLevelId,
                            CreatedDate = ppa.CreatedDate,
                            CreatedById = ppa.CreatedById,
                            ModifiedDate = ppa.ModifiedDate,
                            ModifiedById = ppa.ModifiedById,
                            PermissionLevel = new PermissionLevel
                            {
                                PermissionLevelId = ppa.PermissionLevel.PermissionLevelId,
                                Name = ppa.PermissionLevel.Name,
                                Description = ppa.PermissionLevel.Description,
                                CreatedDate = ppa.PermissionLevel.CreatedDate,
                                CreatedById = ppa.PermissionLevel.CreatedById,
                                ModifiedDate = ppa.PermissionLevel.ModifiedDate,
                                ModifiedById = ppa.PermissionLevel.ModifiedById,
                                RowVersion = ppa.PermissionLevel.RowVersion,
                                PermissionLevelAssignments = ppa.PermissionLevel.PermissionLevelAssignments.Select(pla => new PermissionLevelAssignment
                                {
                                    PermissionLevelId = pla.PermissionLevelId,
                                    BasePermissionId = pla.BasePermissionId,
                                    BasePermission = new BasePermission
                                    {
                                        BasePermissionId = pla.BasePermission.BasePermissionId,
                                        Name = pla.BasePermission.Name,
                                        Description = pla.BasePermission.Description
                                    }
                                }).ToList()
                            }
                        }).ToList()
                })
                .ToList();

            _permissionCache[cacheKey] = (userPermissions, DateTime.UtcNow);
            return userPermissions;
        }

        public async Task UpdatePathPermissionsAsync(string oldPath, string newPath)
        {
            var permissions = await _context.PathPermissions
                .Where(p => p.Path.StartsWith(oldPath))
                .ToListAsync();

            foreach (var perm in permissions)
            {
                perm.Path = perm.Path.Replace(oldPath, newPath);
            }

            await _context.SaveChangesAsync();
            // Invalidate caches after updating paths
            var userId = 0; // Use a default or get from context if needed; since no userId passed, perhaps call without it or adjust
            // But to keep simple, assume invalidation is called separately if needed, or add userId to method signature if required.
            // For now, just clear caches directly
            _permissionCache.Clear();
            _userAppGroupCache.Clear();
        }

        public async Task<bool> HaveSharedBundleAsync(List<int> groupIds1, List<int> groupIds2)
        {
            if (!groupIds1.Any() || !groupIds2.Any()) return false;

            var bundlesForGroup1 = await _context.BundleGroups
                .Where(bg => groupIds1.Contains(bg.AppGroupId))
                .Select(bg => bg.BundleId)
                .ToListAsync();

            var bundlesForGroup2 = await _context.BundleGroups
                .Where(bg => groupIds2.Contains(bg.AppGroupId))
                .Select(bg => bg.BundleId)
                .ToListAsync();

            return bundlesForGroup1.Intersect(bundlesForGroup2).Any();
        }

        private int GetMatchLength(string queryPath, string permPath, bool isGlobal)
        {
            if (isGlobal)
            {
                return permPath == queryPath ? permPath.Length : 0;
            }

            if (!permPath.Contains('(') || !permPath.Contains(')'))
            {
                return queryPath.StartsWith(permPath) ? permPath.Length : 0;
            }

            int start = permPath.IndexOf('(');
            int end = permPath.IndexOf(')', start + 1);
            if (start < 0 || end < 0) return 0;

            string prefix = permPath.Substring(0, start);
            string suffix = permPath.Substring(end + 1);
            string langsStr = permPath.Substring(start + 1, end - start - 1);
            string[] langs = langsStr.Split('|');

            foreach (var lang in langs)
            {
                string expanded = prefix + lang + suffix;
                if (queryPath.StartsWith(expanded))
                {
                    return expanded.Length;
                }
            }

            return 0;
        }
    }
}