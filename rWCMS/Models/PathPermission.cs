using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class PathPermission
    {
        public int PathPermissionId { get; set; }
        [Required, MaxLength(850)]
        public string Path { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public ICollection<PathPermissionAssignment> PathPermissionAssignments { get; set; }
    }
}