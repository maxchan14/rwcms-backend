using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using rWCMS.Enum;

namespace rWCMS.Models
{
    public class ADEntity
    {
        public int ADEntityId { get; set; }
        [Required, MaxLength(100)]
        public string SID { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public EntityType EntityType { get; set; }

        public ICollection<AppGroupMember> AppGroupMembers { get; set; }
    }
}