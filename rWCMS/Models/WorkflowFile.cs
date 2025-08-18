namespace rWCMS.Models
{
    public class WorkflowFile
    {
        public int WorkflowId { get; set; }
        public PublishWorkflow Workflow { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}