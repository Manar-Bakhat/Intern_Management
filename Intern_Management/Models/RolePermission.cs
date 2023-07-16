using System.ComponentModel.DataAnnotations.Schema;

namespace Intern_Management.Models
{
    public class RolePermission
    {
        [ForeignKey("Role")]
        public int? RoleCode { get; set; }
        public Role? Role { get; set; }

        [ForeignKey("Permission")]
        public int? PermissionCode { get; set; }
        public Permission? Permission { get; set; }
    }
}
