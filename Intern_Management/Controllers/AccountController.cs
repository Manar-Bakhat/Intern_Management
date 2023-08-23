using Intern_Management.Data;
using Intern_Management.Models.DTO;
using Intern_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Text;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("RegisterCandidate")]
        public async Task<IActionResult> RegisterCandidate(CandidateRegistrationDTO candidateDTO)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == candidateDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("An account with this email already exists.");
                }

                // Password Confirmation Check
                if (candidateDTO.Password != candidateDTO.ConfirmPassword)
                {
                    return BadRequest("Password and confirm password do not match.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(candidateDTO.Password);

                var candidate = new Candidate
                {
                    FirstName = candidateDTO.FirstName,
                    LastName = candidateDTO.LastName,
                    Gender = candidateDTO.Gender,
                    Email = candidateDTO.Email,
                    Password = hashedPassword,
                    BirthdayDate = candidateDTO.BirthdayDate,
                    City = candidateDTO.City,
                    PhoneNumber = candidateDTO.PhoneNumber
                };

                // Generate the default profile picture based on the user's first name
                if (!string.IsNullOrEmpty(candidate.FirstName))
                {
                    candidate.PicturePath = GenerateDefaultProfilePicture(candidate.FirstName);
                }


                var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
                if (candidateRole == null)
                {
                    return BadRequest("The role 'Candidate' was not found in the database.");
                }
                candidate.Role = candidateRole;

                if (candidateDTO.AcademicQualifications == null || candidateDTO.AcademicQualifications.Count == 0)
                {
                    return BadRequest("Academic qualifications are mandatory.");
                }

                var academicQualifications = new List<AcademicQualification>();
                foreach (var academicQualificationDTO in candidateDTO.AcademicQualifications)
                {
                    var academicQualification = new AcademicQualification
                    {
                        InstitutionName = academicQualificationDTO.InstitutionName,
                        Degree = academicQualificationDTO.Degree,
                        FieldOfStudy = academicQualificationDTO.FieldOfStudy,
                        StartDate = academicQualificationDTO.StartDate,
                        EndDate = academicQualificationDTO.EndDate
                    };

                    academicQualifications.Add(academicQualification);
                }

                candidate.AcademicQualifications = academicQualifications;

                if (candidateDTO.Experiences != null && candidateDTO.Experiences.Count > 0)
                {
                    var experiences = new List<Experience>();
                    foreach (var experienceDTO in candidateDTO.Experiences)
                    {
                        var experience = new Experience
                        {
                            CompanyName = experienceDTO.CompanyName,
                            StartDate = experienceDTO.StartDate,
                            EndDate = experienceDTO.EndDate,
                            Specialisation = experienceDTO.Specialisation,
                            ProjectName = experienceDTO.ProjectName
                        };

                        experiences.Add(experience);
                    }

                    candidate.Experiences = experiences;
                }

                if (candidateDTO.Skills != null && candidateDTO.Skills.Count > 0)
                {
                    var skills = new List<Skill>();
                    foreach (var skillDTO in candidateDTO.Skills)
                    {
                        var skill = new Skill
                        {
                            Name = skillDTO.Name
                        };

                        skills.Add(skill);
                    }

                    candidate.Skills = skills;
                }

                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();

                return Ok("The candidate's account has been successfully created.");
            }

            return BadRequest("Invalid data for candidate account creation.");
        }

        [HttpPost("RegisterSupervisor")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterSupervisor(SupervisorRegistrationDTO supervisorDTO)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == supervisorDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("An account with this email already exists.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(supervisorDTO.Password);

                var supervisor = new Supervisor
                {
                    FirstName = supervisorDTO.FirstName,
                    LastName = supervisorDTO.LastName,
                    Gender = supervisorDTO.Gender,
                    Email = supervisorDTO.Email,
                    Password = hashedPassword,
                    StartDate = supervisorDTO.StartDate,
                    Project = supervisorDTO.Project,
                    Specialisation = supervisorDTO.Specialisation
                };

                // Generate the default profile picture based on the supervisor's first name
                if (!string.IsNullOrEmpty(supervisor.FirstName))
                {
                    supervisor.PicturePath = GenerateDefaultProfilePicture(supervisor.FirstName);
                }

                var supervisorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Supervisor");
                if (supervisorRole == null)
                {
                    return BadRequest("The role 'Supervisor' was not found in the database.");
                }
                supervisor.Role = supervisorRole;

                _context.Supervisors.Add(supervisor);
                await _context.SaveChangesAsync();

                return Ok("The supervisor account has been successfully created.");
            }

            return BadRequest("Invalid data for supervisor account creation.");
        }


        [HttpPost("RegisterAdministrator")]
        public async Task<IActionResult> RegisterAdministrator(AdministratorRegistrationDTO administratorDTO)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == administratorDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("An account with this email already exists.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(administratorDTO.Password);

                var administrator = new User
                {
                    FirstName = administratorDTO.FirstName,
                    LastName = administratorDTO.LastName,
                    Gender = administratorDTO.Gender,
                    Email = administratorDTO.Email,
                    Password = hashedPassword,
                    PicturePath = GenerateDefaultProfilePicture(administratorDTO.FirstName), // Generate the default profile picture
                    RoleId = 1 // Assuming the role ID for administrator is 1
                };

                _context.User.Add(administrator);
                await _context.SaveChangesAsync();

                return Ok("The administrator account has been successfully created.");
            }

            return BadRequest("Invalid data for administrator account creation.");
        }



        private string GenerateDefaultProfilePicture(string firstName)
        {
            // Get the first letter of the first name and convert it to uppercase
            string firstLetter = firstName.Substring(0, 1).ToUpper();

            // You can choose any colors for the circle background
            // Here, we are using a random color from a predefined list
            List<string> circleColors = new List<string> { "#FF5733", "#33FF57", "#5733FF", "#FF57FF", "#33FFFF", "#FFFF33" };
            Random rand = new Random();
            string circleColor = circleColors[rand.Next(circleColors.Count)];

            // Create a SKBitmap with the dimensions of the image (100x100 pixels)
            using (var bitmap = new SKBitmap(100, 100))
            {
                // Create a SKCanvas to draw on the SKBitmap
                using (var canvas = new SKCanvas(bitmap))
                {
                    // Clear the canvas with the circle color
                    canvas.Clear(SKColor.Parse(circleColor));

                    // Create a SKPaint object for the text
                    using (var textPaint = new SKPaint())
                    {
                        textPaint.Color = SKColors.White;
                        textPaint.TextSize = 40;
                        textPaint.TextAlign = SKTextAlign.Center;
                        textPaint.Typeface = SKTypeface.FromFamilyName("Arial"); // Replace with the desired font

                        // Calculate the position to center the text in the circle
                        float x = bitmap.Width / 2;
                        float y = (bitmap.Height + textPaint.TextSize) / 2;

                        // Draw the first letter in the center of the circle
                        canvas.DrawText(firstLetter, x, y, textPaint);
                    }

                    // Convert the SKBitmap to a byte array in JPEG format
                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                    {
                        byte[] jpegBytes = data.ToArray();
                        string base64Image = "data:image/jpeg;base64," + Convert.ToBase64String(jpegBytes);

                        return base64Image;
                    }
                }

            }
        }
    }
}
