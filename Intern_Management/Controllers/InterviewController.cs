using Intern_Management.Data;
using Intern_Management.Models.DTO;
using Intern_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InterviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint to submit interview details for a specific candidate
        [HttpPost("SubmitInterview/{candidateId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SubmitInterview(int candidateId, [FromBody] InterviewDTO interviewDTO)
        {
            // Retrieve specific candidate based on candidate ID
            Candidate? candidate = await _context.Candidates
                .Include(c => c.Interview) // Include the interview associated with the candidate
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
            {
                return BadRequest("The specified candidate was not found.");
            }

            // Create a new Interview object from the interview details sent in InterviewDTO
            var interview = new Interview
            {
                FunctionalSkills = interviewDTO.FunctionalSkills,
                TechnicalSkills = interviewDTO.TechnicalSkills,
                PersonalCompetencies = interviewDTO.PersonalCompetencies,
                LanguageSkills = interviewDTO.LanguageSkills
            };

            // Associate the interview to the specific candidate
            candidate.Interview = interview;

            // Save changes to database
            await _context.SaveChangesAsync();

            // Update the InterviewId field in the Candidate table
            candidate.InterviewId = interview.Id;
            await _context.SaveChangesAsync();

            return Ok("Les détails de l'entretien ont été soumis avec succès.");
        }

        // Endpoint to retrieve interview details for a specific candidate
        [HttpGet("GetInterviewDetails/{candidateId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetInterviewDetails(int candidateId)
        {
            // Retrieve specific candidate based on candidate ID
            Candidate? candidate = await _context.Candidates
                .Include(c => c.Interview) // Include the interview associated with the candidate
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
            {
                return BadRequest("The specified candidate was not found.");
            }

            // Check if the interview exists for the candidate
            if (candidate.Interview == null)
            {
                return NotFound("\r\nInterview details for this candidate have not been submitted.");
            }

            // Resend candidate interview details
            var interviewDTO = new InterviewDTO
            {
                FunctionalSkills = candidate.Interview.FunctionalSkills,
                TechnicalSkills = candidate.Interview.TechnicalSkills,
                PersonalCompetencies = candidate.Interview.PersonalCompetencies,
                LanguageSkills = candidate.Interview.LanguageSkills
            };

            return Ok(interviewDTO);
        }

        // Endpoint to retrieve all interview details
        [HttpGet("GetAllInterviews")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllInterviews()
        {
            try
            {
                var allInterviews = await _context.Interviews
                    .Include(i => i.Candidate) // Include the Candidate associated with the interview
                    .Select(i => new
                    {
                        Interview = i,
                        CandidateName = i.Candidate != null ? new CandidateDTO
                        {
                            FirstName = i.Candidate.FirstName,
                            LastName = i.Candidate.LastName
                        } : null
                    })
                    .ToListAsync();

                return Ok(allInterviews);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the interviews.");
            }
        }

    }
}
