using System.ComponentModel.DataAnnotations;

namespace rWCMS.DTOs
{
    public class AddFileVersionRequest
    {
        public int ItemId { get; set; }

        [Required, MaxLength(850)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
    }
}