using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Intern_Management.Models
{
    public class Certificate
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string? FilePath { get; set; }

        [ForeignKey("Request")]
        public int RequestId { get; set; }
        public Request? Request { get; set; }
    }
}
