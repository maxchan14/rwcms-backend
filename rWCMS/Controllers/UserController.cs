using Microsoft.AspNetCore.Mvc;
using rWCMS.DTOs;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUserAsync();
            return Ok(user);
        }

        [HttpGet("current-user-permissions")]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            var permissions = await _userService.GetCurrentUserPermissionsAsync();
            return Ok(permissions);
        }
    }
}