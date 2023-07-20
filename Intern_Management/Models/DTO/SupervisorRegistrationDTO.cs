using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class SupervisorRegistrationDTO
    {

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public GenderType Gender { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public string? Project { get; set; }

        [Required]
        public SpecialisationType Specialisation { get; set; }
    }
}
