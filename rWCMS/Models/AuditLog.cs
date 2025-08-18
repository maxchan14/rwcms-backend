using System;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class AuditLog
    {
        public int AuditId { get; set; }
        public int? ItemId { get; set; }
        public Item Item { get; set; }
        public int? WorkflowId { get; set; }
        public PublishWorkflow Workflow { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required, MaxLength(100)]
        public string Action { get; set; }
        [MaxLength(1000)]
        public string? Details { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}