using System;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class FileVersion
    {
        public int VersionId { get; set; }
        public int ItemId { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        [Required, MaxLength(850)]
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }

        public Item Item { get; set; }
        public User CreatedBy { get; set; }
    }
}