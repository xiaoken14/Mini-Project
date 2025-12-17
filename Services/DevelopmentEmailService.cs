using HealthcareApp.Models;

namespace HealthcareApp.Services
{
    public class DevelopmentEmailService : IEmailService
    {
        private readonly ILogger<DevelopmentEmailService> _logger;
        private static readonly Dictionary<string, string> _otpStorage = new();

        public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            try
            {
                // Extract OTP from message
                var otpMatch = System.Text.RegularExpressions.Regex.Match(body, @"(\d{6})");
                if (otpMatch.Success)
                {
                    var otp = otpMatch.Groups[1].Value;
                    
                    // Store OTP for retrieval
                    _otpStorage[toEmail] = otp;
                    
                    _logger.LogWarning($"=== DEVELOPMENT MODE ===");
                    _logger.LogWarning($"OTP for {toEmail}: {otp}");
                    _logger.LogWarning($"======================");
                    
                    // Also write to file
                    var filePath = "otp_codes.txt";
                    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Email: {toEmail} - OTP: {otp}\n";
                    await File.AppendAllTextAsync(filePath, logEntry);
                }
                else
                {
                    // Log regular email
                    _logger.LogWarning($"=== DEVELOPMENT EMAIL ===");
                    _logger.LogWarning($"To: {toEmail}");
                    _logger.LogWarning($"Subject: {subject}");
                    _logger.LogWarning($"Body: {body}");
                    _logger.LogWarning($"HTML: {isBodyHtml}");
                    _logger.LogWarning($"========================");
                }
                
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in development email service");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(EmailViewModel emailModel)
        {
            return await SendEmailAsync(emailModel.Email, emailModel.Subject, emailModel.Body, emailModel.IsBodyHtml);
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath, bool isBodyHtml = false)
        {
            _logger.LogWarning($"=== DEVELOPMENT EMAIL WITH ATTACHMENT ===");
            _logger.LogWarning($"To: {toEmail}");
            _logger.LogWarning($"Subject: {subject}");
            _logger.LogWarning($"Body: {body}");
            _logger.LogWarning($"Attachment: {attachmentPath}");
            _logger.LogWarning($"HTML: {isBodyHtml}");
            _logger.LogWarning($"========================================");
            
            await Task.CompletedTask;
            return true;
        }

        public void SendEmail(EmailViewModel emailModel)
        {
            _logger.LogWarning($"=== DEVELOPMENT EMAIL (SYNC) ===");
            _logger.LogWarning($"To: {emailModel.Email}");
            _logger.LogWarning($"Subject: {emailModel.Subject}");
            _logger.LogWarning($"Body: {emailModel.Body}");
            _logger.LogWarning($"HTML: {emailModel.IsBodyHtml}");
            _logger.LogWarning($"===============================");
        }

        public static string? GetOTPForEmail(string email)
        {
            _otpStorage.TryGetValue(email, out var otp);
            return otp;
        }
    }
}