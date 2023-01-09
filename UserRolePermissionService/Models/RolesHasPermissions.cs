namespace UserService.Models
{
    public class RolesHasPermissions
    {
        public long PermissionId { get; set; }
        public Permission Permission { get; set; }

        public long RoleId { get; set; }
        public Role Role { get; set; }
    }
}
