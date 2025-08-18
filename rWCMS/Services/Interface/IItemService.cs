using rWCMS.DTOs;
using rWCMS.Models;
using System.Threading.Tasks;

namespace rWCMS.Services.Interface
{
    public interface IItemService
    {
        Task<Item> GetItemAsync(int itemId);
        Task UnlockFileAsync(int itemId);
        Task AddItemAsync(AddItemRequest request);
        Task UpdateItemAsync(int itemId, UpdateItemRequest request);
        Task DeleteItemAsync(int itemId);
        Task<List<ItemDto>> GetAccessibleItemsAsync(string parentPath);
        Task RelocateItemAsync(int itemId, RelocateItemRequest request);
    }
}