using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models.DTO
{
    public class SendRequestDTO
    {

        [Required(ErrorMessage = "The CV file is required.")]
        public IFormFile? CVFile { get; set; }

        [Required(ErrorMessage = "The motivation letter file is required.")]
        public IFormFile? MotivationLetterFile { get; set; }

        public List<IFormFile>? Certificates { get; set; }

        [Required(ErrorMessage = "The 'InterestedIn' field is required.")]
        public InterestedInType InterestedIn { get; set; }

        [Required(ErrorMessage = "The internship start date is required.")]
        public DateTime StartDateInternship { get; set; }

        [Required(ErrorMessage = "The internship end date is required.")]
        public DateTime EndDateInternship { get; set; }

        [Required(ErrorMessage = "The 'TypeInternship' field is required.")]
        public TypeInternshipType TypeInternship { get; set; }
    }
}
