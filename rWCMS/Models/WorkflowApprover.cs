namespace rWCMS.Models
{
    public class WorkflowApprover
    {
        public int WorkflowId { get; set; }
        public PublishWorkflow Workflow { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}