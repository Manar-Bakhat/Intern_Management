using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class CandidateRegistrationDTO
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
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public DateTime BirthdayDate { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        // Academic qualifications
        public List<AcademicQualificationDTO>? AcademicQualifications { get; set; }

        // Work experiences
        public List<ExperienceDTO>? Experiences { get; set; }

        // Skills
        public List<SkillDTO>? Skills { get; set; }
    }
}
