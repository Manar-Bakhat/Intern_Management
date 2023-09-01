using Intern_Management.Models.DTO;
using Intern_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intern_Management.Data;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public AdministratorController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet("candidates")]
        public IActionResult GetCandidatesWithSupervisors()
        {
            var candidatesWithSupervisors = _context.Candidates
                .Include(c => c.Supervisor) // Include Supervisor navigation property
                .Where(c => c.RoleId == 3)
                .Select(c => new
                {
                    Candidate = new CandidateDTO
                    {
                        Id = c.Id,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Gender = c.Gender,
                        Email = c.Email
                    },
                    SupervisorAssignment = c.SupervisorId != null ? new SupervisorAssignmentDTO
                    {
                        SupervisorId = c.SupervisorId.Value
                    } : null,
                    SupervisorProfile = c.SupervisorId != null ? new SupervisorProfileDTO
                    {
                        FirstName = c.Supervisor.FirstName,
                        LastName = c.Supervisor.LastName,
                        Email = c.Supervisor.Email,
                        StartDate = c.Supervisor.StartDate,
                        Project = c.Supervisor.Project,
                        Specialisation = c.Supervisor.Specialisation
                    } : null
                })
                .ToList();

            return Ok(candidatesWithSupervisors);
        }


        // GET api/candidate/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CandidateProfileDTO>> GetCandidateDetails(int id)
        {
            // Find the candidate by ID
            Candidate? candidate = await _context.Candidates
                .Include(c => c.AcademicQualifications)
                .Include(c => c.Experiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            // Map the relevant fields from the Candidate entity to the CandidateProfileDTO
            var profileDTO = new CandidateProfileDTO
            {
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                BirthdayDate = candidate.BirthdayDate,
                City = candidate.City,
                PhoneNumber = candidate.PhoneNumber,
                AcademicQualifications = candidate.AcademicQualifications?.Select(aq => new AcademicQualificationDTO
                {
                    InstitutionName = aq.InstitutionName,
                    Degree = aq.Degree,
                    FieldOfStudy = aq.FieldOfStudy,
                    StartDate = aq.StartDate,
                    EndDate = aq.EndDate
                }).ToList(),
                Experiences = candidate.Experiences?.Select(exp => new ExperienceDTO
                {
                    CompanyName = exp.CompanyName,
                    StartDate = exp.StartDate,
                    EndDate = exp.EndDate,
                    Specialisation = exp.Specialisation,
                    ProjectName = exp.ProjectName
                }).ToList(),
                Skills = candidate.Skills?.Select(skill => new SkillDTO
                {
                    Name = skill.Name
                }).ToList()
            };

            return Ok(profileDTO);
        }

        [HttpGet("supervisors")]
        public IActionResult GetSupervisors()
        {
            // Retrieve the list of users with role 2 (Supervisor)
            List<Supervisor> supervisors = _context.Supervisors
                .Where(s => s.RoleId == 2)
                .ToList();

            // Convert the Supervisor objects to SupervisorProfileDTO
            List<SupervisorProfileDTO> supervisorDTOs = supervisors.Select(s => new SupervisorProfileDTO
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                StartDate = s.StartDate,
                Project = s.Project,
                Specialisation = s.Specialisation
            }).ToList();

            return Ok(supervisorDTOs);
        }


        [HttpGet("total-candidates")]
        public IActionResult GetTotalCandidates()
        {
            int totalCandidates = _context.Candidates.Count(c => c.RoleId == 3);
            return Ok(new { TotalCandidates = totalCandidates });
        }


        [HttpGet("GetTotalRequests")]
        public IActionResult GetTotalRequests()
        {
            try
            {
                int totalRequests = _context.Requests.Count();
                return Ok(new { TotalRequests = totalRequests });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the total requests.");
            }
        }


        // POST api/administrator/delete-candidate/{id}
        [HttpDelete("delete-candidate/{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            try
            {
                // Find the candidate by ID
                var candidate = await _context.Candidates.FindAsync(id);

                if (candidate == null)
                {
                    return NotFound("Candidate not found.");
                }

                // Remove the candidate
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();

                return Ok("Candidate deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the candidate: {ex.Message}");
            }
        }


        // POST api/administrator/delete-supervisor/{id}
        [HttpDelete("delete-supervisor/{id}")]
        public async Task<IActionResult> DeleteSupervisor(int id)
        {
            try
            {
                // Find the supervisor by ID
                var supervisor = await _context.Supervisors.FindAsync(id);

                if (supervisor == null)
                {
                    return NotFound("Supervisor not found.");
                }

                // Remove the supervisor
                _context.Supervisors.Remove(supervisor);
                await _context.SaveChangesAsync();

                return Ok("Supervisor deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the supervisor: {ex.Message}");
            }
        }


        //
        [HttpGet("total-interviews")]
        public IActionResult GetTotalInterviews()
        {
            try
            {
                int totalInterviews = _context.Interviews.Count();
                return Ok(new { TotalInterviews = totalInterviews });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the total interviews.");
            }
        }

        //
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfileAdministrator()
        {
            // Retrieve the administrator's email from the HttpContext
            var administratorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the administrator record based on the email
            User? administrator = await _context.User
                .FirstOrDefaultAsync(u => u.Email == administratorEmail);

            if (administrator == null || administrator.RoleId != 1) // Assuming role ID 1 is for administrators
            {
                return NotFound("Administrator not found.");
            }

            // Map the administrator's profile data to an AdministratorProfileDTO (you need to create this DTO)
            var profileDTO = new AdministratorProfileDTO
            {
                FirstName = administrator.FirstName,
                LastName = administrator.LastName,
                Email = administrator.Email,

            };

            return Ok(profileDTO);
        }

        // PUT api/administrator/profile
        [HttpPut("profile")]
        public async Task<IActionResult> PutProfileAdministrator([FromBody] AdministratorProfileDTO profileDTO)
        {
            // Retrieve the administrator's email from the HttpContext
            var administratorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the administrator record based on the email
            User? administrator = await _context.User
                .FirstOrDefaultAsync(u => u.Email == administratorEmail);

            if (administrator == null || administrator.RoleId != 1) // Assuming role ID 1 is for administrators
            {
                return NotFound("Administrator not found.");
            }

            // Update the administrator's profile information
            administrator.FirstName = profileDTO.FirstName;
            administrator.LastName = profileDTO.LastName;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Administrator profile updated successfully.");
        }


        // PUT api/administrator/password
        [HttpPut("password")]
        public async Task<IActionResult> PutPassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            // Retrieve the administrator's email from the HttpContext
            var administratorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the administrator record based on the email
            User? administrator = await _context.User
                .FirstOrDefaultAsync(u => u.Email == administratorEmail);

            if (administrator == null || administrator.RoleId != 1) // Assuming role ID 1 is for administrators
            {
                return NotFound("Administrator not found.");
            }

            // Verify the old password matches the current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, administrator.Password))
            {
                return BadRequest("Old password is incorrect.");
            }

            // Check if the new password and confirm password match
            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
            {
                return BadRequest("New password and confirm password do not match.");
            }

            // Hash the new password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);

            // Update the administrator's password
            administrator.Password = hashedPassword;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }


        // PUT api/administrator/picture
        [HttpPut("picture")]
        public async Task<IActionResult> PutAdministratorPicture([FromBody] PictureDTO pictureDTO)
        {
            // Retrieve the administrator's email from the HttpContext
            var administratorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the administrator record based on the email
            User? administrator = await _context.User.FirstOrDefaultAsync(u => u.Email == administratorEmail);

            if (administrator == null || administrator.RoleId != 1) // Assuming role ID 1 is for administrators
            {
                return NotFound("Administrator not found.");
            }

            // Validate the base64 image data
            if (string.IsNullOrEmpty(pictureDTO.Data))
            {
                return BadRequest("Invalid image data.");
            }

            try
            {
                // Convert the base64 string to a byte array
                byte[] imageData = Convert.FromBase64String(pictureDTO.Data);

                // Check if the image size is within the allowed limit (800KB)
                if (imageData.Length > 800 * 1024)
                {
                    return BadRequest("Image size exceeds the allowed limit of 800KB.");
                }

                // Generate a unique file name for the image (you can use GUID or any other method)
                string fileName = Guid.NewGuid().ToString() + ".jpg"; // You can use the appropriate file extension based on the image format

                // Combine the absolute folder path with the file name
                string imagePath = Path.Combine("C:\\Users\\hp\\source\\repos\\Intern_Management\\Intern_Management\\images\\profilePicture", fileName);

                // Save the image to the specified path
                await System.IO.File.WriteAllBytesAsync(imagePath, imageData);

                // Update the PicturePath field with the relative path to the image
                administrator.PicturePath = imagePath;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok("Administrator picture updated successfully.");
            }
            catch (FormatException)
            {
                return BadRequest("Invalid image data format.");
            }
            catch
            {
                // Handle other exceptions if necessary
                return StatusCode(500, "An error occurred while updating administrator picture.");
            }
        }


        // GET api/administrator/picture
        [HttpGet("picture")]
        public IActionResult GetAdministratorPicture()
        {
            // Retrieve the administrator's email from the HttpContext
            var administratorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the administrator record based on the email
            User? administrator = _context.User.FirstOrDefault(u => u.Email == administratorEmail);

            if (administrator == null || string.IsNullOrEmpty(administrator.PicturePath))
            {
                return NotFound("Administrator picture not found.");
            }

            // Read the image file content and convert it to base64
            byte[] imageBytes = System.IO.File.ReadAllBytes(Path.Combine("C:\\Users\\hp\\source\\repos\\Intern_Management\\Intern_Management\\", administrator.PicturePath));
            string base64Image = Convert.ToBase64String(imageBytes);

            // Create a PictureDTO object with the base64 image data
            var pictureDTO = new PictureDTO
            {
                FileName = "administrator_picture.jpg",
                ContentType = "image/jpeg",
                Data = base64Image
            };

            return Ok(pictureDTO);
        }

        // Helper method to get the content type from the image file extension
        private string GetContentTypeFromImageExtension(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream"; // Default content type for unknown file extensions
            }
        }







    }
}
