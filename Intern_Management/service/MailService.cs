using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using MailKit.Security;

namespace Intern_Management.service
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendTestEmailAsync(string recipientEmail)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "sandbox.smtp.mailtrap.io";
            int port = 587; // Use port 587 for StartTls
            SecureSocketOptions socketOptions = SecureSocketOptions.StartTls;
            string username = _configuration["EmailSettings:Username"] ?? "29cc511da7245b";
            string password = _configuration["EmailSettings:Password"] ?? "cc5982a97aff22";

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, socketOptions);
                await client.AuthenticateAsync(username, password);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your Name", username));
                message.To.Add(new MailboxAddress("Recipient Name", recipientEmail));
                message.Subject = "Test Email Subject";
                message.Body = new TextPart("plain")
                {
                    Text = "This is a test email from your ASP.NET Core application using MailKit."
                };

                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
        }
    }
}
