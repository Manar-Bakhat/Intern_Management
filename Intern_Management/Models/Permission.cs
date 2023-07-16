using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Code { get; set; }

        [Required]
        public string? Description { get; set; }

        // Navigation property for RolePermissions
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
