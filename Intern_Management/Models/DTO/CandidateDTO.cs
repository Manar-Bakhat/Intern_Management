namespace Intern_Management.Models.DTO
{
    public class CandidateDTO
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public GenderType Gender { get; set; }

        public string? Email { get; set; }


    }
}
