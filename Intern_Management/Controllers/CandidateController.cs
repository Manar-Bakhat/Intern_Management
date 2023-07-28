using Intern_Management.Data;
using Intern_Management.Models;
using Intern_Management.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Candidate")]
    public class CandidateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CandidateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/candidate/profile
        [HttpGet("profile")]
        public async Task<ActionResult<CandidateProfileDTO>> GetCandidateProfile()
        {
            // Retrieve the user's email from the HttpContext
            var currentUserEmail = HttpContext.Items["Email"]?.ToString();

            // Find the current user's candidate record based on the email
            Candidate? candidate = await _context.Candidates
                .Include(c => c.AcademicQualifications)
                .Include(c => c.Experiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Email == currentUserEmail);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            // Map the relevant fields from the Candidate entity to the CandidateProfileDTO
            var profileDTO = new CandidateProfileDTO
            {
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
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

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateCandidateProfile([FromForm] CandidateProfileDTO profileDTO)
        {
            // Retrieve the user's email from the HttpContext
            var currentUserEmail = HttpContext.Items["Email"]?.ToString();

            // Find the current user's candidate record based on the email
            Candidate? candidate = await _context.Candidates
                .Include(c => c.AcademicQualifications)
                .Include(c => c.Experiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Email == currentUserEmail);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            // Update the candidate's profile information
            candidate.FirstName = profileDTO.FirstName;
            candidate.LastName = profileDTO.LastName;
            candidate.BirthdayDate = profileDTO.BirthdayDate;
            candidate.City = profileDTO.City;
            candidate.PhoneNumber = profileDTO.PhoneNumber;

            

            // Update AcademicQualifications
            if (profileDTO.AcademicQualifications != null)
            {
                foreach (var academicQualificationDTO in profileDTO.AcademicQualifications)
                {
                    if (academicQualificationDTO.Index < candidate.AcademicQualifications?.Count)
                    {
                        var academicQualification = candidate.AcademicQualifications.ElementAt(academicQualificationDTO.Index);
                        academicQualification.InstitutionName = academicQualificationDTO.InstitutionName;
                        academicQualification.Degree = academicQualificationDTO.Degree;
                        academicQualification.FieldOfStudy = academicQualificationDTO.FieldOfStudy;
                        academicQualification.StartDate = academicQualificationDTO.StartDate;
                        academicQualification.EndDate = academicQualificationDTO.EndDate;
                    }
                }
            }

            // Update Experiences
            if (profileDTO.Experiences != null)
            {
                foreach (var experienceDTO in profileDTO.Experiences)
                {
                    if (experienceDTO.Index < candidate.Experiences?.Count)
                    {
                        var experience = candidate.Experiences.ElementAt(experienceDTO.Index);
                        experience.CompanyName = experienceDTO.CompanyName;
                        experience.StartDate = experienceDTO.StartDate;
                        experience.EndDate = experienceDTO.EndDate;
                        experience.Specialisation = experienceDTO.Specialisation;
                        experience.ProjectName = experienceDTO.ProjectName;
                    }
                }
            }

            // Update Skills
            if (profileDTO.Skills != null)
            {
                foreach (var skillDTO in profileDTO.Skills)
                {
                    if (skillDTO.Index < candidate.Skills?.Count)
                    {
                        var skill = candidate.Skills.ElementAt(skillDTO.Index);
                        skill.Name = skillDTO.Name;
                    }
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Candidate profile updated successfully.");
        }


        [HttpPut("password")]
        public async Task<IActionResult> PutPassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            // Retrieve the candidate's email from the HttpContext
            var candidateEmail = HttpContext.Items["Email"]?.ToString();

            // Find the candidate record based on the email
            Candidate? candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.Email == candidateEmail);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            // Verify the old password matches the current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, candidate.Password))
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

            // Update the candidate's password
            candidate.Password = hashedPassword;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }




        // PUT api/Image/picture
        [HttpPut("picture")]
        public async Task<IActionResult> PutCandidatePicture([FromBody] PictureDTO pictureDTO)
        {
            // Retrieve the candidate's email from the HttpContext
            var candidateEmail = HttpContext.Items["Email"]?.ToString();

            // Find the candidate record based on the email
            Candidate? candidate = await _context.Candidates.FirstOrDefaultAsync(s => s.Email == candidateEmail);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
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
                candidate.PicturePath = imagePath;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok("Candidate picture updated successfully.");
            }
            catch (FormatException)
            {
                return BadRequest("Invalid image data format.");
            }
            catch
            {
                // Handle other exceptions if necessary
                return StatusCode(500, "An error occurred while updating candidate picture.");
            }
        }

        [HttpGet("picture")]
        public IActionResult GetCandidatePicture()
        {
            // Retrieve the candidate's email from the HttpContext
            var candidateEmail = HttpContext.Items["Email"]?.ToString();

            // Find the candidate record based on the email
            Candidate? candidate = _context.Candidates.FirstOrDefault(s => s.Email == candidateEmail);

            if (candidate == null || string.IsNullOrEmpty(candidate.PicturePath))
            {
                return NotFound("Candidate picture not found.");
            }

            // Get the base64 image data from the PicturePath field
            string base64ImageData = candidate.PicturePath.Substring(candidate.PicturePath.IndexOf(',') + 1);

            // Create the PictureDTO instance to transfer the image data to the client
            var pictureDTO = new PictureDTO
            {
                FileName = "candidate_picture.jpg", // You can set the file name dynamically based on your implementation
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
