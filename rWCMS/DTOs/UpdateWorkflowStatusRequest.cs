using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class UpdateWorkflowStatusRequest
    {
        [Required, MaxLength(20)]
        public string Status { get; set; }

        public bool TerminationLockByCreator { get; set; }
    }
}