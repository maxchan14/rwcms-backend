using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        [Required, MaxLength(850)]
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public long FileSize { get; set; }
        [MaxLength(10)]
        public string? LockedByType { get; set; }
        public int? LockedById { get; set; }
        public bool PendingDeletion { get; set; }
        public bool DeletedOnProduction { get; set; }
        public bool PendingRename { get; set; }
        [MaxLength(255)]
        public string? PendingName { get; set; }
        public bool PendingMove { get; set; }
        [MaxLength(850)]
        public string? PendingPath { get; set; }
        public bool PublishedToStaging { get; set; }
        [MaxLength(850)]
        public string? StagingPath { get; set; }
        public bool PublishedToProduction { get; set; }
        [MaxLength(850)]
        public string? ProductionPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedById { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
        public ICollection<FileVersion> FileVersions { get; set; }
        public ICollection<WorkflowFile> WorkflowFiles { get; set; }
    }
}