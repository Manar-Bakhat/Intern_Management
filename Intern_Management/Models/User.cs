using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intern_Management.Models
{
    public class User
    {

        [Key]
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        //A one-to-one relationship between the User and Role tables.
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role? Role { get; set; }

    }

    public class Jwt
    {
        internal char[] key;

        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Subject { get; set; }


    }

}