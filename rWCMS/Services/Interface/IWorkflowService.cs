using rWCMS.DTOs;
using rWCMS.Models;
using System.Threading.Tasks;

namespace rWCMS.Services.Interface
{
    public interface IWorkflowService
    {
        Task<PublishWorkflow> GetWorkflowAsync(int workflowId);
        Task CreateWorkflowAsync(CreateWorkflowRequest request);
        Task UpdateWorkflowStatusAsync(int workflowId, UpdateWorkflowStatusRequest request);
    }
}