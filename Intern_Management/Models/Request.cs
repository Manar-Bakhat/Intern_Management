using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Intern_Management.Models
{
    public class Request
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Required(ErrorMessage = "CV File is required")]
        public string? CVFilePath { get; set; }

        [Required(ErrorMessage = "Motivation Letter File is required")]
        public string? MotivationLetterFilePath { get; set; }

        public ICollection<Certificate>? Certificates { get; set; }

        [Required(ErrorMessage = "Your interest is required")]
        public InterestedInType InterestedIn { get; set; }

        [Required(ErrorMessage = "Star tDate Internship is required")]
        public DateTime StartDateInternship { get; set; }

        [Required(ErrorMessage = "End Date Internship is required")]
        public DateTime EndDateInternship { get; set; }

        [Required(ErrorMessage = "Type Internship is required")]
        public TypeInternshipType TypeInternship { get; set; }


        // Navigation property for Candidate
        [ForeignKey("Candidate")]
        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }
    }

    public enum RequestStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "Approved")]
        Approved,
        [EnumMember(Value = "Rejected")]
        Rejected
    }

    public enum InterestedInType
    {
        
        [EnumMember(Value = "Developpement avec DotNet")]
        DotNet,
        [EnumMember(Value = "Developpement avec Java")]
        Java,
        [EnumMember(Value = "Devops")]
        Devops,
        [EnumMember(Value = "SAP")]
        SAP,
        [EnumMember(Value = "TestAutomation")]
        TestAutomation,
        [EnumMember(Value = "Other")]
        Other
    }

    public enum TypeInternshipType
    {
        [EnumMember(Value = "PFE")]
        PFE,
        [EnumMember(Value = "PFA")]
        PFA,
        [EnumMember(Value = "Pre-Employment")]
        preEmployment,
        [EnumMember(Value = "Other")]
        Other
    }
}
