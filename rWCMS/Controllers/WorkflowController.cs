using Microsoft.AspNetCore.Mvc;
using rWCMS.DTOs;
using rWCMS.Services.Interface;
using System.Threading.Tasks;

namespace rWCMS.Controllers
{
    [ApiController]
    [Route("api/v1/workflows")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpGet("{workflowId}")]
        public async Task<IActionResult> GetWorkflow(int workflowId)
        {
            var workflow = await _workflowService.GetWorkflowAsync(workflowId);
            return Ok(workflow);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequest request)
        {
            await _workflowService.CreateWorkflowAsync(request);
            return Ok();
        }

        [HttpPut("{workflowId}/status")]
        public async Task<IActionResult> UpdateWorkflowStatus(int workflowId, [FromBody] UpdateWorkflowStatusRequest request)
        {
            await _workflowService.UpdateWorkflowStatusAsync(workflowId, request);
            return Ok();
        }
    }
}