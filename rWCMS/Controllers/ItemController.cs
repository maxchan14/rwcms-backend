using Microsoft.AspNetCore.Mvc;
using rWCMS.DTOs;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Controllers
{
    [ApiController]
    [Route("api/v1/items")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetItem(int itemId)
        {
            var item = await _itemService.GetItemAsync(itemId);
            return Ok(item);
        }

        [HttpPost("{itemId}/unlock")]
        public async Task<IActionResult> UnlockFile(int itemId)
        {
            await _itemService.UnlockFileAsync(itemId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
        {
            await _itemService.AddItemAsync(request);
            return Ok();
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateItemRequest request)
        {
            await _itemService.UpdateItemAsync(itemId, request);
            return Ok();
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            await _itemService.DeleteItemAsync(itemId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetItemsByPath([FromQuery] string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "/"; // Default to root if no path provided
            }

            var items = await _itemService.GetAccessibleItemsAsync(path);
            return Ok(items);
        }

        [HttpPost("{itemId}/relocate")]
        public async Task<IActionResult> RelocateItem(int itemId, [FromBody] RelocateItemRequest request)
        {
            await _itemService.RelocateItemAsync(itemId, request);
            return Ok();
        }
    }
}