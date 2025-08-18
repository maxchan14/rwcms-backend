namespace rWCMS.Models
{
    public class BundleGroup
    {
        public int BundleId { get; set; }
        public AssociationBundle Bundle { get; set; }
        public int AppGroupId { get; set; }
        public AppGroup AppGroup { get; set; }
    }
}