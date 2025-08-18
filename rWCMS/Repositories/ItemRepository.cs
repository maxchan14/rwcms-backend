using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly rWCMSDbContext _context;

        public ItemRepository(rWCMSDbContext context)
        {
            _context = context;
        }

        public async Task<Item?> GetItemAsync(int itemId)
        {
            return await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == itemId);
        }

        public async Task AddItemAsync(Item item)
        {
            if (string.IsNullOrEmpty(item.Path) || string.IsNullOrEmpty(item.Name))
            {
                throw new ArgumentException("Path and Name must not be null or empty.");
            }
            if (!string.IsNullOrEmpty(item.LockedByType) && item.LockedById == null)
            {
                throw new ArgumentException("LockedById must be non-null when LockedByType is set.");
            }
            if (item.PendingMove && string.IsNullOrEmpty(item.PendingPath))
            {
                throw new ArgumentException("PendingPath must not be null if PendingMove is true.");
            }
            await _context.Items.AddAsync(item);
        }

        public async Task UpdateItemAsync(Item item)
        {
            if (string.IsNullOrEmpty(item.Path) || string.IsNullOrEmpty(item.Name))
            {
                throw new ArgumentException("Path and Name must not be null or empty.");
            }
            if (!string.IsNullOrEmpty(item.LockedByType) && item.LockedById == null)
            {
                throw new ArgumentException("LockedById must be non-null when LockedByType is set.");
            }
            if (item.PendingMove && string.IsNullOrEmpty(item.PendingPath))
            {
                throw new ArgumentException("PendingPath must not be null if PendingMove is true.");
            }
            _context.Items.Update(item);
            await Task.CompletedTask;
        }

        public async Task DeleteItemAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingDeletion = true;
                _context.Items.Update(item);
            }
            await Task.CompletedTask;
        }

        public async Task<List<Item>> GetDirectChildrenAsync(string parentPath)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                parentPath = "/";
            }
            if (!parentPath.EndsWith('/'))
                parentPath += '/';

            return await _context.Items
                .Where(i => i.Path != null && i.Name != null &&
                            i.Path != "" && i.Name != "" &&
                            i.Path.StartsWith(parentPath) &&
                            (i.IsFolder ? i.Path == parentPath + i.Name + "/" : i.Path == parentPath + i.Name))
                .Select(i => new Item
                {
                    ItemId = i.ItemId,
                    CreatedById = i.CreatedById,
                    CreatedDate = i.CreatedDate,
                    FileSize = i.FileSize,
                    IsFolder = i.IsFolder,
                    LockedByType = i.LockedByType,
                    LockedById = i.LockedById,
                    ModifiedById = i.ModifiedById,
                    ModifiedDate = i.ModifiedDate,
                    Name = i.Name,
                    Path = i.Path,
                    RowVersion = i.RowVersion,
                    PendingDeletion = i.PendingDeletion,
                    DeletedOnProduction = i.DeletedOnProduction,
                    PendingRename = i.PendingRename,
                    PendingName = i.PendingName,
                    PendingMove = i.PendingMove,
                    PendingPath = i.PendingPath,
                    PublishedToStaging = i.PublishedToStaging,
                    StagingPath = i.StagingPath,
                    PublishedToProduction = i.PublishedToProduction,
                    ProductionPath = i.ProductionPath,
                    ModifiedBy = new User
                    {
                        UserId = i.ModifiedBy.UserId,
                        Username = i.ModifiedBy.Username
                    }
                })
                .ToListAsync();
        }

        public async Task<List<Item>> GetChildItemsAsync(string parentPath)
        {
            if (!parentPath.EndsWith('/'))
                parentPath += '/';
            return await _context.Items
                .Where(i => i.Path.StartsWith(parentPath))
                .ToListAsync();
        }

        public async Task MarkForDeletionAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingDeletion = true;
                _context.Items.Update(item);
                if (item.IsFolder)
                {
                    var children = await GetChildItemsAsync(item.Path);
                    foreach (var child in children)
                    {
                        child.PendingDeletion = true;
                        _context.Items.Update(child);
                    }
                }
            }
            await Task.CompletedTask;
        }

        public async Task MarkForRenameAsync(int itemId, string newName)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingRename = true;
                item.PendingName = newName;
                _context.Items.Update(item);
            }
            await Task.CompletedTask;
        }

        public async Task RevertRenameAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingRename = false;
                item.PendingName = null;
                item.PublishedToStaging = false;
                item.StagingPath = null;
                _context.Items.Update(item);
                if (item.IsFolder)
                {
                    var children = await GetChildItemsAsync(item.Path);
                    foreach (var child in children)
                    {
                        child.PublishedToStaging = false;
                        child.StagingPath = null;
                        _context.Items.Update(child);
                    }
                }
            }
            await Task.CompletedTask;
        }

        public async Task<Item?> GetItemByPathAsync(string path)
        {
            return await _context.Items
                .FirstOrDefaultAsync(i => i.Path == path);
        }

        public async Task MarkForMoveAsync(int itemId, string newPath)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingMove = true;
                item.PendingPath = newPath;
                _context.Items.Update(item);
            }
            await Task.CompletedTask;
        }

        public async Task RevertMoveAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);
            if (item != null)
            {
                item.PendingMove = false;
                item.PendingPath = null;
                item.PublishedToStaging = false;
                item.StagingPath = null;
                _context.Items.Update(item);
                if (item.IsFolder)
                {
                    var children = await GetChildItemsAsync(item.Path);
                    foreach (var child in children)
                    {
                        child.PublishedToStaging = false;
                        child.StagingPath = null;
                        _context.Items.Update(child);
                    }
                }
            }
            await Task.CompletedTask;
        }
    }
}