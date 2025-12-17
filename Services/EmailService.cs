using System.Net;
using System.Net.Mail;
using HealthcareApp.Models;
using Microsoft.Extensions.Configuration;

namespace HealthcareApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            try
            {
                var emailModel = new EmailViewModel
                {
                    Email = toEmail,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };

                return await SendEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(EmailViewModel emailModel)
        {
            try
            {
                // Read values from appsettings.json file
                string user = _configuration["Smtp:User"] ?? "";
                string pass = _configuration["Smtp:Pass"] ?? "";
                string name = _configuration["Smtp:Name"] ?? "";
                string host = _configuration["Smtp:Host"] ?? "";
                int port = _configuration.GetValue<int>("Smtp:Port");

                // Construct email
                var mail = new MailMessage();
                mail.To.Add(new MailAddress(emailModel.Email, "My Lovely"));
                mail.Subject = emailModel.Subject;
                mail.Body = emailModel.Body;
                mail.IsBodyHtml = emailModel.IsBodyHtml;

                // Set the from address (sender)
                mail.From = new MailAddress(user, name);

                // File attachment (optional)
                if (!string.IsNullOrEmpty(emailModel.AttachmentPath) && File.Exists(emailModel.AttachmentPath))
                {
                    var attachment = new Attachment(emailModel.AttachmentPath);
                    mail.Attachments.Add(attachment);
                }

                // Setup the SMTP client with the username (email) and password
                using var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user, pass)
                };

                // Send the email asynchronously
                await smtp.SendMailAsync(mail);
                
                _logger.LogInformation("Email sent successfully to {Email}", emailModel.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", emailModel.Email);
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath, bool isBodyHtml = false)
        {
            try
            {
                var emailModel = new EmailViewModel
                {
                    Email = toEmail,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml,
                    AttachmentPath = attachmentPath
                };

                return await SendEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with attachment to {Email}", toEmail);
                return false;
            }
        }

        // Synchronous version - Send() method is synchronous
        // We need to wait a few seconds for the operation to complete
        // BUT runtime error message will be shown if the SMTP connection failed
        // Good for debugging purpose
        public void SendEmail(EmailViewModel emailModel)
        {
            try
            {
                // Read values from appsettings.json file
                string user = _configuration["Smtp:User"] ?? "";
                string pass = _configuration["Smtp:Pass"] ?? "";
                string name = _configuration["Smtp:Name"] ?? "";
                string host = _configuration["Smtp:Host"] ?? "";
                int port = _configuration.GetValue<int>("Smtp:Port");

                // Construct email
                var mail = new MailMessage();
                mail.To.Add(new MailAddress(emailModel.Email, "My Lovely"));
                mail.Subject = emailModel.Subject;
                mail.Body = emailModel.Body;
                mail.IsBodyHtml = emailModel.IsBodyHtml;

                // Set the from address (sender)
                mail.From = new MailAddress(user, name);

                // File attachment (optional)
                if (!string.IsNullOrEmpty(emailModel.AttachmentPath) && File.Exists(emailModel.AttachmentPath))
                {
                    var attachment = new Attachment(emailModel.AttachmentPath);
                    mail.Attachments.Add(attachment);
                }

                // Setup the SMTP client with the username (email) and password
                using var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user, pass)
                };

                // Send the email synchronously
                smtp.Send(mail);
                
                _logger.LogInformation("Email sent successfully to {Email}", emailModel.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", emailModel.Email);
                throw; // Re-throw for debugging purposes in synchronous version
            }
        }
    }
}