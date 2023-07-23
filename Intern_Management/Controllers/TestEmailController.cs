using Intern_Management.service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEmailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public TestEmailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpGet("SendTestEmail")]
        public async Task<IActionResult> SendTestEmail(string recipientEmail)
        {
            try
            {
                // Call the SendTestEmailAsync method from the MailService
                await _mailService.SendTestEmailAsync(recipientEmail);

                return Ok("Test email sent successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during email sending
                return BadRequest($"Failed to send test email: {ex.Message}");
            }
        }
    }
}
