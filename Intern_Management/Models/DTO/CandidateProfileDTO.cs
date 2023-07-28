using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class CandidateProfileDTO
    {

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime BirthdayDate { get; set; }

        public string? City { get; set; }

        public string? PhoneNumber { get; set; }

        // Academic qualifications
        public List<AcademicQualificationDTO>? AcademicQualifications { get; set; }

        // Work experiences
        public List<ExperienceDTO>? Experiences { get; set; }

        // Skills
        public List<SkillDTO>? Skills { get; set; }

    }
}
