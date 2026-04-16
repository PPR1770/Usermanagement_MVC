using Microsoft.AspNetCore.Identity;

namespace UserManagementApp.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
