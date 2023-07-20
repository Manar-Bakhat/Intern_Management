using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class AcademicQualification
    {
        [Key]
        public int Id { get; set; }

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

        // Navigation property for Candidate
        [ForeignKey("Candidate")]
        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }
    }
}
