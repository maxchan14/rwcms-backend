using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class AssociationBundle
    {
        public int BundleId { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedById { get; set; }

        public User CreatedBy { get; set; }
        public ICollection<BundleGroup> BundleGroups { get; set; }
    }
}