using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class BasePermission
    {
        public int BasePermissionId { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}