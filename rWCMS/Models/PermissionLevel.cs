using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class PermissionLevel
    {
        public int PermissionLevelId { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedById { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
        public ICollection<PermissionLevelAssignment> PermissionLevelAssignments { get; set; }
        public ICollection<PathPermissionAssignment> PathPermissionAssignments { get; set; }
    }
}