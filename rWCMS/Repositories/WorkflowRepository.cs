using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly rWCMSDbContext _context;

        public WorkflowRepository(rWCMSDbContext context)
        {
            _context = context;
        }

        public async Task<PublishWorkflow> GetWorkflowAsync(int workflowId)
        {
            return await _context.PublishWorkflows.FindAsync(workflowId);
        }

        public async Task AddWorkflowAsync(PublishWorkflow workflow)
        {
            await _context.PublishWorkflows.AddAsync(workflow);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWorkflowAsync(PublishWorkflow workflow)
        {
            _context.PublishWorkflows.Update(workflow);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkflowFile>> GetWorkflowFilesAsync(int workflowId)
        {
            return await _context.WorkflowFiles
                .Where(wf => wf.WorkflowId == workflowId)
                .Include(wf => wf.Item)
                .ToListAsync();
        }

        public async Task AddWorkflowFileAsync(WorkflowFile workflowFile)
        {
            await _context.WorkflowFiles.AddAsync(workflowFile);
            await _context.SaveChangesAsync();
        }
    }
}