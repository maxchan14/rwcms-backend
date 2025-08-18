using rWCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IItemRepository
    {
        Task<Item?> GetItemAsync(int itemId);
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(int itemId);
        Task<List<Item>> GetDirectChildrenAsync(string parentPath);
        Task<List<Item>> GetChildItemsAsync(string parentPath);
        Task MarkForDeletionAsync(int itemId);
        Task MarkForRenameAsync(int itemId, string newName);
        Task RevertRenameAsync(int itemId);
        Task<Item?> GetItemByPathAsync(string path);
        Task MarkForMoveAsync(int itemId, string newPath);
        Task RevertMoveAsync(int itemId);
    }
}