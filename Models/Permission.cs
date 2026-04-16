namespace UserManagementApp.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public ApplicationRole? Role { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
