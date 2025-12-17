using HealthcareApp.Models;

namespace HealthcareApp.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false);
        Task<bool> SendEmailAsync(EmailViewModel emailModel);
        Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath, bool isBodyHtml = false);
        void SendEmail(EmailViewModel emailModel); // Synchronous version
    }
}