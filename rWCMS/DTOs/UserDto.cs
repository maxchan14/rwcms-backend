using System;

namespace rWCMS.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string SID { get; set; }
        public string AdLoginId { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsSiteAdmin { get; set; }
    }
}