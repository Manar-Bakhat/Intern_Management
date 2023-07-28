using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class ExperienceDTO
    {
        public int Index { get; set; } // Add Index property to identify the element

        public string? CompanyName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Specialisation { get; set; }

        [Required]
        public string? ProjectName { get; set; }
    }
}
