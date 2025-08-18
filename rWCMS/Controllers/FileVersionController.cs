using Microsoft.AspNetCore.Mvc;
using rWCMS.DTOs;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Controllers
{
    [ApiController]
    [Route("api/v1/fileversions")]
    public class FileVersionController : ControllerBase
    {
        private readonly IFileVersionService _fileVersionService;

        public FileVersionController(IFileVersionService fileVersionService)
        {
            _fileVersionService = fileVersionService;
        }

        [HttpGet("{fileVersionId}")]
        public async Task<IActionResult> GetFileVersion(int fileVersionId)
        {
            var fileVersion = await _fileVersionService.GetFileVersionAsync(fileVersionId);
            return Ok(fileVersion);
        }

        [HttpPost]
        public async Task<IActionResult> AddFileVersion([FromBody] AddFileVersionRequest request)
        {
            await _fileVersionService.AddFileVersionAsync(request);
            return Ok();
        }
    }
}