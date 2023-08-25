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



    }
}
