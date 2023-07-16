using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Code { get; set; }

        [Required]
        public string? Name { get; set; }

        // Navigation property for RolePermissions
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
