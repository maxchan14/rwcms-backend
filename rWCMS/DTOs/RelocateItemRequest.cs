using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class RelocateItemRequest
    {
        [Required, MaxLength(850)]
        public string TargetPath { get; set; }
    }
}