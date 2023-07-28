namespace Intern_Management.Models.DTO
{
    public class SupervisorProfileDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime StartDate { get; set; }
        public string? Project { get; set; }
        public SpecialisationType Specialisation { get; set; }
    }
}
