using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rWCMS.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string SID { get; set; }
        [Required, MaxLength(100)]
        public string AdLoginId { get; set; }
        [Required, MaxLength(100)]
        public string Username { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsSiteAdmin { get; set; }
    }
}