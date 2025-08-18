namespace rWCMS.DTOs
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool PendingDeletion { get; set; }
        public bool DeletedOnProduction { get; set; }
        public bool PendingRename { get; set; }
        public string? PendingName { get; set; }
        public bool PendingMove { get; set; }
        public string? PendingPath { get; set; }
        public bool PublishedToStaging { get; set; }
        public bool PublishedToProduction { get; set; }
        public string? LockedByType { get; set; }
        public int? LockedById { get; set; }
        public int ModifiedById { get; set; }
        public string ModifiedByUsername { get; set; }
    }
}