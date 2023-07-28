using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intern_Management.Data;
using Intern_Management.Models;
using Intern_Management.Models.DTO;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetAssignedCandidates")]
        public IActionResult GetAssignedCandidates()
        {
            // Retrieve currently logged in supervisor ID from identity claims
            var supervisorIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(supervisorIdString) || !int.TryParse(supervisorIdString, out var supervisorId))
            {
                return BadRequest("Supervisor ID not found or invalid format.");
            }

            // Retrieve the supervisor currently logged in with the candidates assigned to this supervisor
            var supervisor = _context.Supervisors
                .Include(s => s.AssignedCandidates) // Load candidates assigned to this supervisor
                .FirstOrDefault(s => s.Id == supervisorId);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
            }

            return Ok(supervisor.AssignedCandidates); // Refer nominated candidates to this supervisor
        }


        //
        // GET api/supervisor/profile
        [HttpGet("profile")]
        public IActionResult GetProfileSupervisor()
        {
            // Retrieve the supervisor's email from the HttpContext
            var supervisorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the supervisor record based on the email
            Supervisor? supervisor = _context.Supervisors
                .FirstOrDefault(s => s.Email == supervisorEmail);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
            }

            // Map the supervisor's profile data to the SupervisorProfileDTO
            var profileDTO = new SupervisorProfileDTO
            {
                FirstName = supervisor.FirstName,
                LastName = supervisor.LastName,
                Email = supervisor.Email,
                StartDate = supervisor.StartDate,
                Project = supervisor.Project,
                Specialisation = supervisor.Specialisation
            };

            return Ok(profileDTO);
        }

        //
        // PUT api/supervisor/profile
        [HttpPut("profile")]
        public async Task<IActionResult> PutProfileSupervisor([FromBody] SupervisorProfileDTO profileDTO)
        {
            // Retrieve the supervisor's email from the HttpContext
            var supervisorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the supervisor record based on the email
            Supervisor? supervisor = await _context.Supervisors
                .FirstOrDefaultAsync(s => s.Email == supervisorEmail);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
            }

            // Update the supervisor's profile information
            supervisor.FirstName = profileDTO.FirstName;
            supervisor.LastName = profileDTO.LastName;
            supervisor.StartDate = profileDTO.StartDate;
            supervisor.Project = profileDTO.Project;
            supervisor.Specialisation = profileDTO.Specialisation;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Supervisor profile updated successfully.");
        }


        // PUT api/supervisor/password
        [HttpPut("password")]
        public async Task<IActionResult> PutPassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            // Retrieve the supervisor's email from the HttpContext
            var supervisorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the supervisor record based on the email
            Supervisor? supervisor = await _context.Supervisors
                .FirstOrDefaultAsync(s => s.Email == supervisorEmail);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
            }

            // Verify the old password matches the current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, supervisor.Password))
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

            // Update the supervisor's password
            supervisor.Password = hashedPassword;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }


  

        // PUT api/Image/picture
        [HttpPut("picture")]
        public async Task<IActionResult> PutSupervisorPicture([FromBody] PictureDTO pictureDTO)
        {
            // Retrieve the supervisor's email from the HttpContext
            var supervisorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the supervisor record based on the email
            Supervisor? supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == supervisorEmail);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
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

                // Combine the folder path with the file name
                string imagePath = Path.Combine("images", "profilePicture", fileName);

                // Save the image to the specified path
                await System.IO.File.WriteAllBytesAsync(imagePath, imageData);

                // Update the PicturePath field with the relative path to the image
                supervisor.PicturePath = imagePath;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok("Supervisor picture updated successfully.");
            }
            catch (FormatException)
            {
                return BadRequest("Invalid image data format.");
            }
            catch
            {
                // Handle other exceptions if necessary
                return StatusCode(500, "An error occurred while updating supervisor picture.");
            }
        }

        // GET api/Image/picture
        [HttpGet("picture")]
        public IActionResult GetSupervisorPicture()
        {
            // Retrieve the supervisor's email from the HttpContext
            var supervisorEmail = HttpContext.Items["Email"]?.ToString();

            // Find the supervisor record based on the email
            Supervisor? supervisor = _context.Supervisors.FirstOrDefault(s => s.Email == supervisorEmail);

            if (supervisor == null || string.IsNullOrEmpty(supervisor.PicturePath))
            {
                return NotFound("Supervisor picture not found.");
            }

            // Get the base64 image data from the PicturePath field
            string base64ImageData = supervisor.PicturePath.Substring(supervisor.PicturePath.IndexOf(',') + 1);

            // Create the PictureDTO instance to transfer the image data to the client
            var pictureDTO = new PictureDTO
            {
                FileName = "supervisor_picture.jpg", // You can set the file name dynamically based on your implementation
                ContentType = "image/jpeg", // Set the correct content type based on the image format
                Data = base64ImageData // Store the base64 image data in the Data property
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