using System.Collections.Generic;

namespace rWCMS.DTOs
{
    public class PathPermissionDto
    {
        public string Path { get; set; }
        public List<string> Permissions { get; set; }
    }
}