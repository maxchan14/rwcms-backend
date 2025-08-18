using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class AppGroup
    {
        public int AppGroupId { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedById { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
        public ICollection<AppGroupMember> AppGroupMembers { get; set; }
        public ICollection<PathPermissionAssignment> PathPermissionAssignments { get; set; }
        public ICollection<BundleGroup> BundleGroups { get; set; }
    }
}