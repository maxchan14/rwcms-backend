using rWCMS.DTOs;
using rWCMS.Enum;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Services
{
    public class FileVersionService : IFileVersionService
    {
        private readonly IFileVersionRepository _fileVersionRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FileVersionService(
            IFileVersionRepository fileVersionRepository,
            IItemRepository itemRepository,
            IPermissionRepository permissionRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _fileVersionRepository = fileVersionRepository;
            _itemRepository = itemRepository;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<FileVersion> GetFileVersionAsync(int fileVersionId)
        {
            var fileVersion = await _fileVersionRepository.GetFileVersionAsync(fileVersionId);
            var item = await _itemRepository.GetItemAsync(fileVersion.ItemId);
            await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Read);
            return fileVersion;
        }

        public async Task AddFileVersionAsync(AddFileVersionRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var item = await _itemRepository.GetItemAsync(request.ItemId);
                if (item == null)
                    throw new ArgumentException($"Item not found: Item with ID {request.ItemId}");
                await _permissionRepository.RequirePermissionAsync(item.Path, BasePermissionType.Edit);
                var userId = await _userRepository.GetCurrentUserIdAsync();
                var fileVersion = new FileVersion
                {
                    ItemId = request.ItemId,
                    MajorVersion = request.MajorVersion,
                    MinorVersion = request.MinorVersion,
                    FilePath = request.FilePath,
                    FileSize = request.FileSize,
                    IsPublished = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = userId
                };
                await _fileVersionRepository.AddFileVersionAsync(fileVersion);
                await _auditLogRepository.AddAuditLogAsync(new AuditLog
                {
                    ItemId = request.ItemId,
                    UserId = userId,
                    Action = "AddVersion",
                    Details = $"Added version {fileVersion.MajorVersion}.{fileVersion.MinorVersion} for item {request.ItemId}",
                    CreatedDate = DateTime.UtcNow
                });

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