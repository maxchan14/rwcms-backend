using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using rWCMS.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public ItemService(IItemRepository itemRepository, IUnitOfWork unitOfWork, IPermissionRepository permissionRepository, IUserRepository userRepository, IAuditLogRepository auditLogRepository)
        {
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Item> GetItemAsync(int itemId)
        {
            var item = await _itemRepository.GetItemAsync(itemId);
            if (item == null)
                throw new ArgumentException($"Item not found: Item with ID {itemId}");
            await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Read);
            return item;
        }

        public async Task UnlockFileAsync(int itemId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var item = await _itemRepository.GetItemAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item not found: Item with ID {itemId}");
                var userId = await _userRepository.GetCurrentUserIdAsync();
                var sid = await _userRepository.GetCurrentUserSidAsync();

                if (item.LockedByType != "User")
                {
                    throw new InvalidOperationException("Can only unlock user-locked files.");
                }

                if (item.LockedById == userId)
                {
                    await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Edit);
                }
                else
                {
                    var effectivePerms = await _permissionRepository.GetEffectivePermissionsAsync(item.Path, sid);

                    bool hasUnlockAllFiles = false;
                    bool hasUnlockGroupFiles = false;
                    var grantingGroups = new HashSet<int>();

                    foreach (var perm in effectivePerms)
                    {
                        foreach (var assignment in perm.PathPermissionAssignments)
                        {
                            var permissions = assignment.PermissionLevel.PermissionLevelAssignments.Select(pla => pla.BasePermission.Name).ToList();
                            if (permissions.Contains(BasePermissionType.UnlockAllFiles.ToString()))
                            {
                                hasUnlockAllFiles = true;
                            }
                            if (permissions.Contains(BasePermissionType.UnlockGroupFiles.ToString()))
                            {
                                hasUnlockGroupFiles = true;
                                grantingGroups.Add(assignment.AppGroupId);
                            }
                        }
                    }

                    var locker = await _userRepository.GetUserAsync(item.LockedById.Value);
                    var lockerAppGroups = await _userRepository.GetUserAppGroupIdsAsync(locker.SID);

                    bool canUnlock = false;

                    if (hasUnlockAllFiles)
                    {
                        canUnlock = true;
                    }
                    else if (hasUnlockGroupFiles)
                    {
                        bool directShare = grantingGroups.Intersect(lockerAppGroups).Any();
                        bool bundleShare = await _permissionRepository.HaveSharedBundleAsync(grantingGroups.ToList(), lockerAppGroups);
                        if (directShare || bundleShare)
                        {
                            canUnlock = true;
                        }
                    }

                    if (!canUnlock)
                    {
                        throw new UnauthorizedAccessException("Permission denied to unlock this file.");
                    }
                }

                item.LockedByType = null;
                item.LockedById = null;
                await _itemRepository.UpdateItemAsync(item);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = itemId,
                    UserId = userId,
                    Action = "Unlock",
                    Details = $"Unlocked item {item.Name} at path {item.Path}",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task AddItemAsync(AddItemRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _permissionRepository.RequirePermissionAsync(request.Path, BasePermissionType.Add);
                var userId = await _userRepository.GetCurrentUserIdAsync();
                var item = new Item
                {
                    Name = request.Name,
                    Path = request.Path,
                    IsFolder = request.IsFolder,
                    FileSize = request.FileSize,
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = userId,
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedById = userId
                };
                await _itemRepository.AddItemAsync(item);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = item.ItemId,
                    UserId = userId,
                    Action = "Add",
                    Details = $"Added item {request.Name} at path {request.Path}",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateItemAsync(int itemId, UpdateItemRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var item = await _itemRepository.GetItemAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item not found: Item with ID {itemId}");
                await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Edit);
                var userId = await _userRepository.GetCurrentUserIdAsync();

                item.Name = request.Name;
                item.Path = request.Path;
                item.IsFolder = request.IsFolder;
                item.FileSize = request.FileSize;
                item.PendingName = request.PendingName;
                item.ModifiedDate = DateTime.UtcNow;
                item.ModifiedById = userId;
                await _itemRepository.UpdateItemAsync(item);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = itemId,
                    UserId = userId,
                    Action = "Update",
                    Details = $"Updated item {item.Name} at path {item.Path}",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteItemAsync(int itemId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var item = await _itemRepository.GetItemAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item not found: Item with ID {itemId}");
                if (item.PendingRename || item.PendingMove)
                    throw new InvalidOperationException("Cannot delete item while a pending change exists.");
                await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Delete);
                var userId = await _userRepository.GetCurrentUserIdAsync();

                item.LockedByType = "User";
                item.LockedById = userId;
                await _itemRepository.MarkForDeletionAsync(itemId);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = itemId,
                    UserId = userId,
                    Action = "MarkForDeletion",
                    Details = $"Marked item {item.Name} for deletion at path {item.Path}",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<ItemDto>> GetAccessibleItemsAsync(string parentPath)
        {
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var userAppGroupIds = await _permissionRepository.GetUserAppGroupIdsAsync(sid);
            if (!await _permissionRepository.HasPermissionAsync(userAppGroupIds, parentPath, BasePermissionType.Read, sid))
                throw new UnauthorizedAccessException($"Permission {BasePermissionType.Read} denied for path {parentPath}");

            var directChildren = await _itemRepository.GetDirectChildrenAsync(parentPath);
            var accessibleItems = new List<ItemDto>();

            foreach (var item in directChildren)
            {
                if (await _permissionRepository.HasPermissionAsync(userAppGroupIds, item.Path, BasePermissionType.Read, sid))
                {
                    accessibleItems.Add(new ItemDto
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Path = item.Path,
                        IsFolder = item.IsFolder,
                        FileSize = item.FileSize,
                        CreatedDate = item.CreatedDate,
                        ModifiedDate = item.ModifiedDate,
                        PendingDeletion = item.PendingDeletion,
                        DeletedOnProduction = item.DeletedOnProduction,
                        PendingRename = item.PendingRename,
                        PendingName = item.PendingName,
                        PendingMove = item.PendingMove,
                        PendingPath = item.PendingPath,
                        PublishedToStaging = item.PublishedToStaging,
                        PublishedToProduction = item.PublishedToProduction,
                        LockedByType = item.LockedByType,
                        LockedById = item.LockedById,
                        ModifiedById = item.ModifiedById,
                        ModifiedByUsername = item.ModifiedBy?.Username ?? string.Empty
                    });
                }
            }

            return accessibleItems;
        }

        public async Task RelocateItemAsync(int itemId, RelocateItemRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var item = await _itemRepository.GetItemAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item not found: Item with ID {itemId}");
                if (item.PendingDeletion || item.PendingRename || item.PendingMove)
                    throw new InvalidOperationException("Cannot relocate item while a pending change exists.");
                await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Edit);

                var targetPath = item.IsFolder && !request.TargetPath.EndsWith("/") ? request.TargetPath + "/" : request.TargetPath;
                var targetParentPath = targetPath.Substring(0, targetPath.LastIndexOf('/') + 1);
                await _permissionRepository.RequirePermissionAsync(targetParentPath, BasePermissionType.Add);

                var existingAtTarget = await _itemRepository.GetItemByPathAsync(targetPath);
                if (existingAtTarget != null)
                    throw new ArgumentException($"Target path {targetPath} already exists.");

                var userId = await _userRepository.GetCurrentUserIdAsync();
                item.LockedByType = "User";
                item.LockedById = userId;
                await _itemRepository.MarkForMoveAsync(itemId, targetPath);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = itemId,
                    UserId = userId,
                    Action = "MarkForMove",
                    Details = $"Marked item {item.Name} for move from {item.Path} to {targetPath}",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}