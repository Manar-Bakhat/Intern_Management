using Intern_Management.Data;
using Intern_Management.Models.DTO;
using Intern_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Registration Method of Candidate

        [HttpPost("RegisterCandidate")]
        public async Task<IActionResult> RegisterCandidate(CandidateRegistrationDTO candidateDTO)
        {
            if (ModelState.IsValid)
            {
                // Check if the candidate's email is already in use
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == candidateDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("An account with this email already exists.");
                }

                // Encrypt the candidate's password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(candidateDTO.Password);

                // Create a new Candidate object from DTO data
                var candidate = new Candidate
                {
                    FirstName = candidateDTO.FirstName,
                    LastName = candidateDTO.LastName,
                    Gender = candidateDTO.Gender,
                    Email = candidateDTO.Email,
                    Password = hashedPassword, // encrypted password
                    BirthdayDate = candidateDTO.BirthdayDate,
                    City = candidateDTO.City,
                    PhoneNumber = candidateDTO.PhoneNumber
                };

                // Find the "Candidate" role in the database
                var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == 3); // ID 3 corresponds to the "Candidate" role

                if (candidateRole == null)
                {
                    return BadRequest("The role 'Candidate' was not found in the database.");
                }

                // Assign the "Candidate" role ID to the candidate
                candidate.RoleId = candidateRole.Id;

                // Check if academic qualifications are provided
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

                // Check if experiences are provided
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

                // Check if skills are provided
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

                // Register the candidate in the database
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();

                return Ok("The candidate's account has been successfully created.");
            }

            return BadRequest("Données invalides pour la création du compte du candidat.");
        }

        // Registration Method of Supervisor

        [HttpPost("RegisterSupervisor")]
        public async Task<IActionResult> RegisterSupervisor(SupervisorRegistrationDTO supervisorDTO)
        {
            if (ModelState.IsValid)
            {
                // Check if supervisor email is already in use
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == supervisorDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("An account with this email already exists.");
                }

                // Encrypt password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(supervisorDTO.Password);

                // Create a new Supervisor object from the DTO data
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

                // Find the "Supervisor" role in the database
                var supervisorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == 2); // ID 2 corresponds to the "Supervisor" role

                if (supervisorRole == null)
                {
                    return BadRequest("The role 'Supervisor' was not found in the database.");
                }

                // Assign the "Supervisor" role ID to the supervisor
                supervisor.RoleId = supervisorRole.Id;

                // Register the supervisor in the database
                _context.Supervisors.Add(supervisor);
                await _context.SaveChangesAsync();

                return Ok("The supervisor account has been successfully created.");
            }

            return BadRequest("Invalid data for supervisor account creation.");
        }


    }
}
