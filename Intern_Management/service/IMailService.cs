namespace Intern_Management.service
{
    public interface IMailService
    {
        Task SendTestEmailAsync(string recipientEmail);
    }
}
