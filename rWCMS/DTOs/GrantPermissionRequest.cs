using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class GrantPermissionRequest
    {
        [Required, MaxLength(850)]
        public string Path { get; set; }

        public int AppGroupId { get; set; }

        public int PermissionLevelId { get; set; }
    }
}