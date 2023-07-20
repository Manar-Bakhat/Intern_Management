using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Intern_Management.Models
{
    public class User
    {

        [Key]
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public GenderType Gender { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        public string? PicturePath { get; set; }

        //A one-to-one relationship between the User and Role tables.
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role? Role { get; set; }

    }

    public enum GenderType
    {
        [EnumMember(Value = "Male")]
        Male,
        [EnumMember(Value = "Female")]
        Female
    }

    public class Jwt
    {
        internal char[]? key;

        public string? Key { get; set; }

        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public string? Subject { get; set; }


    }

}