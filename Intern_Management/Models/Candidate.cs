using Azure.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intern_Management.Models
{

    [Table("Candidates")]
    public class Candidate : User
    {
        [Required(ErrorMessage = "Birthday Date is required")]
        public DateTime BirthdayDate { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string? PhoneNumber { get; set; }


        // Navigation property for AcademicQualifications
        public ICollection<AcademicQualification>? AcademicQualifications { get; set; }

        // Navigation property for Experiences
        public ICollection<Experience>? Experiences { get; set; }

        // Navigation property for Skills
        public ICollection<Skill>? Skills { get; set; }

        // Navigation property for Requests


        public int RequestId { get; set; }
        public Request? Request { get; set; }

        // Navigation property for Interview


        public int InterviewId { get; set; }
        public Interview? Interview { get; set; }

        // Navigation property for Supervisor
        [ForeignKey("Supervisor")]
        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
    }
}
