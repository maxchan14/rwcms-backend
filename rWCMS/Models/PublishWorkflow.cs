using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using rWCMS.Enum;

namespace rWCMS.Models
{
    public class PublishWorkflow
    {
        public int WorkflowId { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(850)]
        public string? Description { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public WorkflowStatus Status { get; set; }
        public bool TerminationLockByCreator { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedById { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
        public ICollection<WorkflowApprover> WorkflowApprovers { get; set; }
        public ICollection<WorkflowFile> WorkflowFiles { get; set; }
    }
}