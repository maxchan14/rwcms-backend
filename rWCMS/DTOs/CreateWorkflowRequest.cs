using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class CreateWorkflowRequest
    {
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(850)]
        public string? Description { get; set; }
        public DateTime? ScheduleTime { get; set; }

        public List<int> ApproverIds { get; set; }

        public List<int> FileIds { get; set; }
    }
}