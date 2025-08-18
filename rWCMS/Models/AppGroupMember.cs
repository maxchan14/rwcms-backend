namespace rWCMS.Models
{
    public class AppGroupMember
    {
        public int AppGroupId { get; set; }
        public AppGroup AppGroup { get; set; }
        public int ADEntityId { get; set; }
        public ADEntity ADEntity { get; set; }
    }
}