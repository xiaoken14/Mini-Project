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

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Extract OTP from message
            var otpMatch = System.Text.RegularExpressions.Regex.Match(message, @"(\d{6})");
            if (otpMatch.Success)
            {
                var otp = otpMatch.Groups[1].Value;
                
                // Store OTP for retrieval
                _otpStorage[email] = otp;
                
                _logger.LogWarning($"=== DEVELOPMENT MODE ===");
                _logger.LogWarning($"OTP for {email}: {otp}");
                _logger.LogWarning($"======================");
                
                // Also write to file
                var filePath = "otp_codes.txt";
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Email: {email} - OTP: {otp}\n";
                await File.AppendAllTextAsync(filePath, logEntry);
            }
            
            await Task.CompletedTask;
        }

        public static string? GetOTPForEmail(string email)
        {
            _otpStorage.TryGetValue(email, out var otp);
            return otp;
        }
    }
}