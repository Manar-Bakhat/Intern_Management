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

        public DateTime CreatedDate { get; set; }


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

        [EnumMember(Value = "BackendDeveloper Java")]
        BackendDeveloperJava,
        [EnumMember(Value = "BackendDeveloper DotNet")]
        BackendDeveloperDotNet,
        [EnumMember(Value = "FrontendDeveloper Angular")]
        FrontendDeveloperAngular,
        [EnumMember(Value = "FrontendDeveloper ReactJS")]
        FrontendDeveloperReactJS,
        [EnumMember(Value = "FullStackDeveloper Java")]
        FullStackDeveloperJava,
        [EnumMember(Value = "FullStackDeveloper DotNet")]
        FullStackDeveloperDotNet,
        [EnumMember(Value = "Tester")]
        Tester,
        [EnumMember(Value = "ProjectManager")]
        ProjectManager,
        [EnumMember(Value = "DataAnalytics")]
        DataAnalytics,
        [EnumMember(Value = "BigData")]
        BigData,
        [EnumMember(Value = "Business Intelligence")]
        BusinessIntelligence,
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
