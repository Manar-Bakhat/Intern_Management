using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class AcademicQualificationDTO
    {
        public int Index { get; set; } // Add Index property to identify the element

        [Required]
        public string? InstitutionName { get; set; }

        [Required]
        public string? Degree { get; set; }

        [Required]
        public string? FieldOfStudy { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
