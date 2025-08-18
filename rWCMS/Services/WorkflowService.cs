using Microsoft.Extensions.Configuration;
using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using rWCMS.Services.Interface;
using rWCMS.Utilities;
using System;
using System.Threading.Tasks;

namespace rWCMS.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IConfiguration _configuration;
        private readonly SftpUtility _sftpUtility;

        public WorkflowService(
            IWorkflowRepository workflowRepository,
            IItemRepository itemRepository,
            IUnitOfWork unitOfWork,
            IPermissionRepository permissionRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IConfiguration configuration,
            SftpUtility sftpUtility)
        {
            _workflowRepository = workflowRepository;
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _configuration = configuration;
            _sftpUtility = sftpUtility;
        }

        public async Task<PublishWorkflow> GetWorkflowAsync(int workflowId)
        {
            var workflow = await _workflowRepository.GetWorkflowAsync(workflowId);
            // Check view permission based on ownership/group/all
            var userId = await _userRepository.GetCurrentUserIdAsync();
            var sid = await _userRepository.GetCurrentUserSidAsync();
            var userAppGroups = await _userRepository.GetUserAppGroupIdsAsync(sid);
            var creatorAppGroups = await _userRepository.GetUserAppGroupIdsAsync(workflow.CreatedBy.SID); // Assume CreatedBy has SID
            var shareDirect = userAppGroups.Intersect(creatorAppGroups).Any();
            var shareBundle = await _permissionRepository.HaveSharedBundleAsync(userAppGroups, creatorAppGroups);
            var isOwn = workflow.CreatedById == userId;
            var isGroup = shareDirect || shareBundle;

            if (await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ViewAllWorkflows))
            {
                return workflow;
            }
            else if (isGroup && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ViewGroupWorkflows))
            {
                return workflow;
            }
            else if (isOwn && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ViewOwnWorkflows))
            {
                return workflow;
            }
            throw new UnauthorizedAccessException("Permission denied to view workflow");
        }

        public async Task CreateWorkflowAsync(CreateWorkflowRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _permissionRepository.RequirePermissionAsync("#Workflow", BasePermissionType.CreateWorkflows);
                var userId = await _userRepository.GetCurrentUserIdAsync();
                var workflow = new PublishWorkflow
                {
                    Title = request.Title,
                    Description = request.Description,
                    ScheduleTime = request.ScheduleTime,
                    Status = WorkflowStatus.Draft,
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = userId,
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedById = userId
                };
                await _workflowRepository.AddWorkflowAsync(workflow);

                if (request.FileIds != null && request.FileIds.Count > 0)
                {
                    foreach (var itemId in request.FileIds)
                    {
                        var item = await _itemRepository.GetItemAsync(itemId);
                        if (item != null)
                        {
                            await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Edit);
                            item.LockedByType = "Workflow";
                            item.LockedById = workflow.WorkflowId;
                            await _itemRepository.UpdateItemAsync(item);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = itemId,
                                UserId = userId,
                                Action = "Lock",
                                Details = $"Locked item {itemId} to workflow {workflow.WorkflowId}",
                                CreatedDate = DateTime.UtcNow
                            });
                            var workflowFile = new WorkflowFile
                            {
                                WorkflowId = workflow.WorkflowId,
                                ItemId = itemId
                            };
                            await _workflowRepository.AddWorkflowFileAsync(workflowFile);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateWorkflowStatusAsync(int workflowId, UpdateWorkflowStatusRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var workflow = await _workflowRepository.GetWorkflowAsync(workflowId);
                if (workflow == null)
                    throw new ArgumentException($"Workflow not found: Workflow with ID {workflowId}");
                // Check update/approve permission
                var userId = await _userRepository.GetCurrentUserIdAsync();
                var sid = await _userRepository.GetCurrentUserSidAsync();
                var userAppGroups = await _userRepository.GetUserAppGroupIdsAsync(sid);
                var creatorAppGroups = await _userRepository.GetUserAppGroupIdsAsync(workflow.CreatedBy.SID);
                var shareDirect = userAppGroups.Intersect(creatorAppGroups).Any();
                var shareBundle = await _permissionRepository.HaveSharedBundleAsync(userAppGroups, creatorAppGroups);
                var isOwn = workflow.CreatedById == userId;
                var isGroup = shareDirect || shareBundle;
                var newStatus = System.Enum.Parse<WorkflowStatus>(request.Status);

                bool canUpdate = false;
                if (await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.UpdateAllWorkflows))
                {
                    canUpdate = true;
                }
                else if (isGroup && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.UpdateGroupWorkflows))
                {
                    canUpdate = true;
                }
                else if (isOwn && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.UpdateOwnWorkflows))
                {
                    canUpdate = true;
                }

                bool canApprove = false;
                if (newStatus == WorkflowStatus.AwaitingApproval || newStatus == WorkflowStatus.Completed || newStatus == WorkflowStatus.Rejected)
                {
                    if (await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ApproveAllWorkflows))
                    {
                        canApprove = true;
                    }
                    else if (isGroup && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ApproveGroupWorkflows))
                    {
                        canApprove = true;
                    }
                    else if (isOwn && await _permissionRepository.HasPermissionAsync("#Workflow", BasePermissionType.ApproveOwnWorkflows))
                    {
                        canApprove = true;
                    }
                }
                else
                {
                    canApprove = true; // Non-approval statuses use update perms
                }

                if (!canUpdate || !canApprove)
                {
                    throw new UnauthorizedAccessException("Permission denied to update workflow status");
                }

                workflow.Status = newStatus;
                workflow.ModifiedDate = DateTime.UtcNow;
                workflow.ModifiedById = userId;

                if (newStatus == WorkflowStatus.Completed)
                {
                    await PublishToProductionAsync(workflow);
                }
                else if (newStatus == WorkflowStatus.AwaitingApproval)
                {
                    await PublishToStagingAsync(workflow);
                }
                else if (newStatus == WorkflowStatus.Rejected)
                {
                    await RejectWorkflowAsync(workflow);
                }

                await _workflowRepository.UpdateWorkflowAsync(workflow);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    WorkflowId = workflowId,
                    UserId = userId,
                    Action = "UpdateStatus",
                    Details = $"Updated workflow {workflowId} status to {request.Status}",
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

        private async Task PublishToStagingAsync(PublishWorkflow workflow)
        {
            var stagingUrl = _configuration["WorkflowSettings:StagingUrl"];
            var userId = await _userRepository.GetCurrentUserIdAsync();
            var workflowFiles = await _workflowRepository.GetWorkflowFilesAsync(workflow.WorkflowId);

            foreach (var wf in workflowFiles)
            {
                var item = wf.Item;
                if (item.PendingDeletion)
                {
                    if (item.PublishedToStaging && item.StagingPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.DeleteDirectoryAsync(item.StagingPath, "Staging");
                        else
                            await _sftpUtility.DeleteFileAsync(item.StagingPath, "Staging");
                    }
                    item.PublishedToStaging = true;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "DeleteOnStaging",
                        Details = $"Deleted item {item.Name} on staging at path {item.StagingPath ?? item.Path}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(item.Path);
                        foreach (var child in children)
                        {
                            child.PublishedToStaging = true;
                            child.StagingPath = child.Path.Replace(item.Path, stagingUrl);
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "DeleteOnStaging",
                                Details = $"Deleted child item {child.Name} on staging at path {child.StagingPath}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (item.PendingRename)
                {
                    var newPath = item.Path.Substring(0, item.Path.LastIndexOf('/') + 1) + item.PendingName + (item.IsFolder ? "/" : "");
                    if (item.PublishedToStaging && item.StagingPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.RenameDirectoryAsync(item.StagingPath, newPath, "Staging");
                        else
                            await _sftpUtility.RenameFileAsync(item.StagingPath, newPath, "Staging");
                    }
                    item.PublishedToStaging = true;
                    item.StagingPath = newPath;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "RenameOnStaging",
                        Details = $"Renamed item {item.Name} to {item.PendingName} on staging at path {newPath}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(item.Path);
                        foreach (var child in children)
                        {
                            child.PublishedToStaging = true;
                            child.StagingPath = child.Path.Replace(item.Path, newPath);
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "RenameOnStaging",
                                Details = $"Updated child item {child.Name} staging path to {child.StagingPath}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (item.PendingMove)
                {
                    var newPath = item.PendingPath;
                    if (item.PublishedToStaging && item.StagingPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.RenameDirectoryAsync(item.StagingPath, newPath, "Staging");
                        else
                            await _sftpUtility.RenameFileAsync(item.StagingPath, newPath, "Staging");
                    }
                    item.PublishedToStaging = true;
                    item.StagingPath = newPath;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "MoveOnStaging",
                        Details = $"Moved item {item.Name} on staging to {newPath}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(item.Path);
                        foreach (var child in children)
                        {
                            child.PublishedToStaging = true;
                            child.StagingPath = child.Path.Replace(item.Path, newPath);
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "MoveOnStaging",
                                Details = $"Updated child item {child.Name} staging path to {child.StagingPath}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }

        private async Task PublishToProductionAsync(PublishWorkflow workflow)
        {
            var productionUrl = _configuration["WorkflowSettings:ProductionUrl"];
            var userId = await _userRepository.GetCurrentUserIdAsync();
            var workflowFiles = await _workflowRepository.GetWorkflowFilesAsync(workflow.WorkflowId);

            foreach (var wf in workflowFiles)
            {
                var item = wf.Item;
                string oldPath = item.Path;
                if (item.PendingDeletion)
                {
                    if (item.PublishedToProduction && item.ProductionPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.DeleteDirectoryAsync(item.ProductionPath, "Production");
                        else
                            await _sftpUtility.DeleteFileAsync(item.ProductionPath, "Production");
                    }
                    item.DeletedOnProduction = true;
                    item.PublishedToProduction = true;
                    item.LockedByType = null;
                    item.LockedById = null;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "DeleteOnProduction",
                        Details = $"Deleted item {item.Name} on production at path {item.ProductionPath ?? item.Path}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(item.Path);
                        foreach (var child in children)
                        {
                            child.DeletedOnProduction = true;
                            child.PublishedToProduction = true;
                            child.LockedByType = null;
                            child.LockedById = null;
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "DeleteOnProduction",
                                Details = $"Deleted child item {child.Name} on production at path {child.ProductionPath ?? child.Path}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (item.PendingRename)
                {
                    var newPath = item.Path.Substring(0, item.Path.LastIndexOf('/') + 1) + item.PendingName + (item.IsFolder ? "/" : "");
                    if (item.PublishedToProduction && item.ProductionPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.RenameDirectoryAsync(item.ProductionPath, newPath, "Production");
                        else
                            await _sftpUtility.RenameFileAsync(item.ProductionPath, newPath, "Production");
                    }
                    item.Name = item.PendingName;
                    item.Path = newPath;
                    item.PendingRename = false;
                    item.PendingName = null;
                    item.PublishedToProduction = true;
                    item.ProductionPath = newPath;
                    item.LockedByType = null;
                    item.LockedById = null;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "RenameOnProduction",
                        Details = $"Renamed item {item.Name} to {item.PendingName} on production at path {newPath}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(oldPath);
                        foreach (var child in children)
                        {
                            child.Path = child.Path.Replace(oldPath, newPath);
                            child.ProductionPath = child.Path;
                            child.PublishedToProduction = true;
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "RenameOnProduction",
                                Details = $"Updated child item {child.Name} production path to {child.ProductionPath}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }

                        await _permissionRepository.UpdatePathPermissionsAsync(oldPath, newPath);
                    }
                }
                else if (item.PendingMove)
                {
                    var newPath = item.PendingPath;
                    if (item.PublishedToProduction && item.ProductionPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.RenameDirectoryAsync(item.ProductionPath, newPath, "Production");
                        else
                            await _sftpUtility.RenameFileAsync(item.ProductionPath, newPath, "Production");
                    }
                    item.Path = newPath;
                    item.PendingMove = false;
                    item.PendingPath = null;
                    item.PublishedToProduction = true;
                    item.ProductionPath = newPath;
                    item.LockedByType = null;
                    item.LockedById = null;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "MoveOnProduction",
                        Details = $"Moved item {item.Name} on production to {newPath}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(oldPath);
                        foreach (var child in children)
                        {
                            child.Path = child.Path.Replace(oldPath, newPath);
                            child.ProductionPath = child.Path;
                            child.PublishedToProduction = true;
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "MoveOnProduction",
                                Details = $"Updated child item {child.Name} production path to {child.ProductionPath}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }

                        await _permissionRepository.UpdatePathPermissionsAsync(oldPath, newPath);
                    }
                }
            }
        }

        private async Task RejectWorkflowAsync(PublishWorkflow workflow)
        {
            var userId = await _userRepository.GetCurrentUserIdAsync();
            var workflowFiles = await _workflowRepository.GetWorkflowFilesAsync(workflow.WorkflowId);

            foreach (var wf in workflowFiles)
            {
                var item = wf.Item;
                if (item.PendingDeletion)
                {
                    item.PendingDeletion = false;
                    item.PublishedToStaging = false;
                    item.StagingPath = null;
                    item.LockedByType = null;
                    item.LockedById = null;
                    await _itemRepository.UpdateItemAsync(item);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "RevertDeletion",
                        Details = $"Reverted deletion for item {item.Name} at path {item.Path}",
                        CreatedDate = DateTime.UtcNow
                    });

                    if (item.IsFolder)
                    {
                        var children = await _itemRepository.GetChildItemsAsync(item.Path);
                        foreach (var child in children)
                        {
                            child.PendingDeletion = false;
                            child.PublishedToStaging = false;
                            child.StagingPath = null;
                            child.LockedByType = null;
                            child.LockedById = null;
                            await _itemRepository.UpdateItemAsync(child);
                            await _auditLogRepository.AddAuditLogAsync(new AuditLog
                            {
                                ItemId = child.ItemId,
                                UserId = userId,
                                Action = "RevertDeletion",
                                Details = $"Reverted deletion for child item {child.Name} at path {child.Path}",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (item.PendingRename)
                {
                    await _sftpUtility.RenameFileAsync(item.StagingPath, item.Path, "Staging");
                    await _itemRepository.RevertRenameAsync(item.ItemId);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "RevertRename",
                        Details = $"Reverted rename for item {item.Name} at path {item.Path}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
                else if (item.PendingMove)
                {
                    if (item.PublishedToStaging && item.StagingPath != null)
                    {
                        if (item.IsFolder)
                            await _sftpUtility.RenameDirectoryAsync(item.StagingPath, item.Path, "Staging");
                        else
                            await _sftpUtility.RenameFileAsync(item.StagingPath, item.Path, "Staging");
                    }
                    await _itemRepository.RevertMoveAsync(item.ItemId);
                    await _auditLogRepository.AddAuditLogAsync(new AuditLog
                    {
                        ItemId = item.ItemId,
                        UserId = userId,
                        Action = "RevertMove",
                        Details = $"Reverted move for item {item.Name} at path {item.Path}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
        }
    }
}