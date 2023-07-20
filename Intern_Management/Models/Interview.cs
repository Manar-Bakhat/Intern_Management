using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class Interview
    {
        [Key]
        public int Id { get; set; }

        public FunctionalSkills FunctionalSkills { get; set; }

        public TechnicalSkills TechnicalSkills { get; set; }

        public PersonalCompetencies PersonalCompetencies { get; set; }

        public LanguageSkills LanguageSkills { get; set; }

        // Navigation property for Candidate

        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }
    }

    public enum FunctionalSkills
    {
        ExceedsExpectations,
        DoesNotMeetExpectations
    }
    public enum TechnicalSkills
    {
        ExceedsExpectations,
        DoesNotMeetExpectations
    }
    public enum PersonalCompetencies
    {
        ExceedsExpectations,
        DoesNotMeetExpectations
    }
    public enum LanguageSkills
    {
        ExceedsExpectations,
        DoesNotMeetExpectations
    }
}
