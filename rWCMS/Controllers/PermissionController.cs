using Microsoft.AspNetCore.Mvc;
using rWCMS.DTOs;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Controllers
{
    [ApiController]
    [Route("api/v1/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> GetPermissions(string path, bool includeInherited = false)
        {
            var permissions = await _permissionService.GetPathPermissionsAsync(path, includeInherited);
            return Ok(permissions);
        }

        [HttpGet("effective/{path}")]
        public async Task<IActionResult> GetEffectivePermissions(string path)
        {
            var permissions = await _permissionService.GetEffectivePermissionsAsync(path);
            return Ok(permissions);
        }

        [HttpPost("break-inheritance/{path}")]
        public async Task<IActionResult> BreakInheritance(string path)
        {
            await _permissionService.BreakInheritanceAsync(path);
            return Ok();
        }

        [HttpPost("revert-inheritance/{path}")]
        public async Task<IActionResult> RevertToInheritance(string path)
        {
            await _permissionService.RevertToInheritanceAsync(path);
            return Ok();
        }

        [HttpPost("grant")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionRequest request)
        {
            await _permissionService.GrantPermissionAsync(request);
            return Ok();
        }
    }
}