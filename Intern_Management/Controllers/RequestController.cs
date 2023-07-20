using Intern_Management.Data;
using Intern_Management.Models;
using Intern_Management.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("SendRequest")]
        [Authorize]
        public async Task<IActionResult> SendRequest([FromForm] SendRequestDTO requestDTO)
        {
            // Retrieve the user's email from the HttpContext
            var currentUserEmail = HttpContext.Items["Email"]?.ToString();

            // Find the current user's candidate record based on the email
            Candidate? currentCandidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Email == currentUserEmail);

            if (currentCandidate == null)
            {
                return BadRequest("Current user is not a candidate.");
            }

            if (ModelState.IsValid)
            {
                // Create a new Request instance from the DTO data
                var request = new Request
                {
                    Status = RequestStatus.Pending,
                    InterestedIn = requestDTO.InterestedIn,
                    StartDateInternship = requestDTO.StartDateInternship,
                    EndDateInternship = requestDTO.EndDateInternship,
                    TypeInternship = requestDTO.TypeInternship,
                    CandidateId = currentCandidate.Id // Associate the request with the current candidate
                };

                // Save CV file if provided
                if (requestDTO.CVFile != null && requestDTO.CVFile.Length > 0)
                {
                    request.CVFilePath = await SaveFile(requestDTO.CVFile);
                }
                else
                {
                    return BadRequest("The CV file is required.");
                }

                // Save motivation letter file if provided
                if (requestDTO.MotivationLetterFile != null && requestDTO.MotivationLetterFile.Length > 0)
                {
                    request.MotivationLetterFilePath = await SaveFile(requestDTO.MotivationLetterFile);
                }
                else
                {
                    return BadRequest("The motivation letter file is required.");
                }

                // Enregistrer les fichiers Certificates s'ils existent
                if (requestDTO.Certificates != null && requestDTO.Certificates.Count > 0)
                {
                    List<Certificate> certificates = new List<Certificate>();

                    foreach (var certificateFile in requestDTO.Certificates)
                    {
                        string certificateFilePath = await SaveFile(certificateFile);

                        var certificate = new Certificate
                        {
                            FilePath = certificateFilePath,
                            Request = request
                        };

                        certificates.Add(certificate);
                    }

                    request.Certificates = certificates;
                }


                // Save the request to the database
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                return Ok("The request has been sent successfully.");
            }

            return BadRequest("Invalid data for sending the request.");
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            // Generate unique file name
            string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;

            // Define the path where to save the file
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(directoryPath); // Create the directory if it does not exist
            string filePath = Path.Combine(directoryPath, fileName);

            // Save file to server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return saved file path
            return filePath;
        }


        // Method To allow the administrator to retrieve all requests
        [HttpGet("GetAllRequests")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                // Configure JsonSerializerOptions with ReferenceHandler.Preserve
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true // Optional: To format the JSON output with indentation
                };

                var allRequests = await _context.Requests
                    .Include(r => r.Candidate)
                    .ToListAsync();

                // Serialize the result to JSON using JsonSerializerOptions
                var jsonResult = JsonSerializer.Serialize(allRequests, options);

                return Content(jsonResult, "application/json");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the requests.");
            }
        }



        // Endpoint to update the status of a job application
        [HttpPut("UpdateRequestStatus/{requestId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, [FromBody] RequestStatusDTO requestStatusDTO)
        {
            // Retrieve specific request based on request ID
            Request? request = await _context.Requests.FindAsync(requestId);

            if (request == null)
            {
                return BadRequest("The specified request was not found.");
            }

            // Update the status of the request with the new status specified
            request.Status = requestStatusDTO.Status;

            // Save changes to database
            await _context.SaveChangesAsync();

            return Ok("The request status has been successfully updated.");
        }



        [HttpPost("AssignSupervisorToCandidate/{candidateId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignSupervisorToCandidate(int candidateId, [FromBody] SupervisorAssignmentDTO supervisorAssignmentDTO)
        {
            // Récupérer le candidat spécifique en fonction de l'ID du candidat
            Candidate? candidate = await _context.Candidates
                .Include(c => c.Request) // Assurez-vous d'inclure la demande associée au candidat
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
            {
                return BadRequest("Le candidat spécifié n'a pas été trouvé.");
            }

            // Vérifier si la demande du candidat est approuvée
            if (candidate.Request?.Status != RequestStatus.Approved)
            {
                return BadRequest("Le candidat doit avoir une demande approuvée pour lui affecter un superviseur.");
            }

            // Récupérer le superviseur spécifique en fonction de l'ID du superviseur envoyé dans SupervisorAssignmentDTO
            Supervisor? supervisor = await _context.Supervisors.FindAsync(supervisorAssignmentDTO.SupervisorId);

            if (supervisor == null)
            {
                return BadRequest("Le superviseur spécifié n'a pas été trouvé.");
            }

            // Affecter le superviseur au candidat spécifique
            candidate.SupervisorId = supervisor.Id; // Correction ici

            // Enregistrer les changements dans la base de données
            await _context.SaveChangesAsync();

            return Ok("Le superviseur a été affecté au candidat avec succès.");
        }






    }
}
