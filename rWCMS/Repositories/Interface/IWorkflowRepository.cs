using rWCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IWorkflowRepository
    {
        Task<PublishWorkflow> GetWorkflowAsync(int workflowId);
        Task AddWorkflowAsync(PublishWorkflow workflow);
        Task UpdateWorkflowAsync(PublishWorkflow workflow);
        Task<List<WorkflowFile>> GetWorkflowFilesAsync(int workflowId);
        Task AddWorkflowFileAsync(WorkflowFile workflowFile);
    }
}