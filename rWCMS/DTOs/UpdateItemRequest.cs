using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class UpdateItemRequest
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required, MaxLength(850)]
        public string Path { get; set; }

        public bool IsFolder { get; set; }

        public long FileSize { get; set; }

        [MaxLength(255)]
        public string? PendingName { get; set; }
    }
}