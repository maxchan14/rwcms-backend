namespace rWCMS.Models
{
    public class PermissionLevelAssignment
    {
        public int PermissionLevelId { get; set; }
        public PermissionLevel PermissionLevel { get; set; }
        public int BasePermissionId { get; set; }
        public BasePermission BasePermission { get; set; }
    }
}