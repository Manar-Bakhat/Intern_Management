using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class Experience
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Specialisation is required")]
        public string? Specialisation { get; set; }

        [Required(ErrorMessage = "Project Name is required")]
        public string? ProjectName { get; set; }

        // Navigation property for Candidate
        [ForeignKey("Candidate")]
        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }
    }
}
