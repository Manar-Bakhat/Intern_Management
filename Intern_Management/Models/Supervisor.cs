using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Intern_Management.Models
{

    [Table("Supervisors")]
    public class Supervisor : User
    {

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; }

        public string? Project { get; set; }

        [Required(ErrorMessage = "Specialisation is required")]
        public SpecialisationType Specialisation { get; set; }

        // Navigation property for Candidates assigned to the Supervisor
        public ICollection<Candidate>? AssignedCandidates { get; set; }
    }

    public enum SpecialisationType
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
}

