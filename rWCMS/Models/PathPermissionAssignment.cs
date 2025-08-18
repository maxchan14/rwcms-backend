using System;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class PathPermissionAssignment
    {
        public int PathPermissionId { get; set; }
        public PathPermission PathPermission { get; set; }
        public int AppGroupId { get; set; }
        public AppGroup AppGroup { get; set; }
        public int PermissionLevelId { get; set; }
        public PermissionLevel PermissionLevel { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedById { get; set; }
        public User ModifiedBy { get; set; }
    }
}