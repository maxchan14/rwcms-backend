using System;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class SystemSetting
    {
        public int SettingId { get; set; }
        public DateTime SystemPermissionUpdate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int LastUpdatedById { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User LastUpdatedBy { get; set; }
    }
}