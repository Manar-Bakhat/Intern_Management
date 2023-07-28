using Intern_Management.Data;
using Intern_Management.Models;
using Intern_Management.Models.DTO;
using Intern_Management.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RequestController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("SendRequest")]
        [Authorize(Roles = "Candidate")]
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


            // Check if the candidate already has a pending request
            bool hasPendingRequest = await _context.Requests.AnyAsync(r => r.CandidateId == currentCandidate.Id && r.Status == RequestStatus.Pending);

            if (hasPendingRequest)
            {
                return BadRequest("You already have a pending request. You cannot submit another request until the current one is processed.");
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

                // Save Certificates files if provided
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

                // Update the RequestId field in the Candidate table
                currentCandidate.RequestId = request.Id;
                await _context.SaveChangesAsync();

                // Check if the candidate's email is not null before sending the email
                if (!string.IsNullOrEmpty(currentCandidate.Email))
                {
                    // Send an email to the candidate with the status "Pending"
                    await SendEmailToCandidate(currentCandidate.Email, RequestStatus.Pending, currentCandidate);
                }


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

            // Check if the status has actually changed
            if (request.Status == requestStatusDTO.Status)
            {
                return Ok("The request status is already set to the specified value.");
            }

            // Update the status of the request with the new status specified
            request.Status = requestStatusDTO.Status;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Check if the new status is "Approved" or "Rejected"
            if (requestStatusDTO.Status == RequestStatus.Approved || requestStatusDTO.Status == RequestStatus.Rejected)
            {
                // Retrieve the candidate associated with the request
                Candidate? candidate = await _context.Candidates.FindAsync(request.CandidateId);

                // Send the email to the candidate with the updated status
                if (candidate != null && !string.IsNullOrEmpty(candidate.Email))
                {
                    await SendEmailToCandidate(candidate.Email, requestStatusDTO.Status, candidate);
                }
            }

            return Ok("The request status has been successfully updated.");
        }

        private async Task SendEmailToCandidate(string candidateEmail, RequestStatus newStatus, Candidate candidate)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "sandbox.smtp.mailtrap.io";
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            SecureSocketOptions socketOptions = SecureSocketOptions.StartTls;
            string username = _configuration["EmailSettings:Username"] ?? "29cc511da7245b";
            string password = _configuration["EmailSettings:Password"] ?? "cc5982a97aff22";

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, socketOptions);
                await client.AuthenticateAsync(username, password);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Administrator", username));
                message.To.Add(new MailboxAddress("Candidate", candidateEmail));
                message.Subject = "Internship Application Update";

                // Customize the email body based on the new status
                string emailBody = ""; 
                if (newStatus == RequestStatus.Approved)
                {
                    emailBody = $"Dear {candidate.FirstName},\r\n\r\nThank you for your interest in NTT DATA. I have received your internship application and carefully reviewed your candidacy.\r\n\r\nI am delighted to inform you that your profile has caught our attention, and we would like to further discuss this opportunity with you. Could you kindly provide us with your availability this week for an interview? This will allow us to get to know each other better and discuss potential areas of collaboration.\r\n\r\nLooking forward to hearing from you. Have a great day!\r\n\r\nBest regards,";
                }
                else if (newStatus == RequestStatus.Rejected)
                {
                    emailBody = $"Dear {candidate.FirstName},\r\n\r\nThank you for applying for the internship position at NTT DATA. After careful consideration, we regret to inform you that your application was not selected for further advancement.\r\n\r\nWe appreciate your interest in joining our team, and we wish you the best in your future endeavors.\r\n\r\nBest regards,";
                }
                else if (newStatus == RequestStatus.Pending)
                {
                    emailBody = $"Dear {candidate.FirstName},\r\n\r\nThis is to confirm that we have received your application for the internship position at NTT DATA. Thank you for your interest in joining our team.\r\n\r\nWe are currently reviewing all applications and will provide an update on your candidacy soon.\r\n\r\nBest regards,";
                }

                message.Body = new TextPart("plain")
                {
                    Text = emailBody
                };

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }



        //
        [HttpPost("AssignSupervisorToCandidate/{candidateId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignSupervisorToCandidate(int candidateId, [FromBody] SupervisorAssignmentDTO supervisorAssignmentDTO)
        {
            // Retrieve specific candidate based on candidate ID
            Candidate? candidate = await _context.Candidates
                .Include(c => c.Request) // Be sure to include the application associated with the candidate
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
            {
                return BadRequest("The specified candidate was not found.");
            }

            // Check if the candidate's application is approved
            if (candidate.Request?.Status != RequestStatus.Approved)
            {
                return BadRequest("\r\nCandidate must have an approved request to assign a supervisor.");
            }

            // Retrieve the specific supervisor based on the supervisor ID sent in SupervisorAssignmentDTO
            Supervisor? supervisor = await _context.Supervisors.FindAsync(supervisorAssignmentDTO.SupervisorId);

            if (supervisor == null)
            {
                return BadRequest("The specified supervisor was not found.");
            }

            // Assign the supervisor to the specific candidate
            candidate.SupervisorId = supervisor.Id;

            // Save changes to database
            await _context.SaveChangesAsync();

            return Ok("The supervisor has been successfully assigned to the candidate.");
        }




    }
}
