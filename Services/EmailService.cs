using System.Net;
using System.Net.Mail;

namespace HealthcareApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // Log the email details for debugging
                _logger.LogInformation($"Sending email to: {email}");
                _logger.LogInformation($"Subject: {subject}");
                
                // Check if SMTP settings are configured
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = _configuration["EmailSettings:Port"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];

                if (string.IsNullOrEmpty(smtpServer) || smtpServer == "smtp.gmail.com" && 
                    (string.IsNullOrEmpty(username) || username == "your-email@gmail.com"))
                {
                    // SMTP not configured, log the email content for development
                    _logger.LogWarning("SMTP not configured. Email content:");
                    _logger.LogWarning($"To: {email}");
                    _logger.LogWarning($"Subject: {subject}");
                    _logger.LogWarning($"Body: {message}");
                    
                    // For development, also write to a file so you can see the OTP
                    var otpMatch = System.Text.RegularExpressions.Regex.Match(message, @"(\d{6})");
                    if (otpMatch.Success)
                    {
                        var otp = otpMatch.Groups[1].Value;
                        var filePath = "otp_codes.txt";
                        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Email: {email} - OTP: {otp}\n";
                        await File.AppendAllTextAsync(filePath, logEntry);
                        _logger.LogWarning($"OTP {otp} saved to {filePath} for testing");
                    }
                    
                    return;
                }

                // Send actual email
                using var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = int.Parse(port!),
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                
                mailMessage.To.Add(email);
                
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                
                // Fallback: save OTP to file for development
                var otpMatch = System.Text.RegularExpressions.Regex.Match(message, @"(\d{6})");
                if (otpMatch.Success)
                {
                    var otp = otpMatch.Groups[1].Value;
                    var filePath = "otp_codes.txt";
                    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Email: {email} - OTP: {otp} (Email failed)\n";
                    await File.AppendAllTextAsync(filePath, logEntry);
                    _logger.LogWarning($"Email failed. OTP {otp} saved to {filePath} for testing");
                }
                
                throw; // Re-throw to let the controller handle the error
            }
        }
    }
}