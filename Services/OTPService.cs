namespace HealthcareApp.Services
{
    public class OTPService : IOTPService
    {
        private readonly Random _random;

        public OTPService()
        {
            _random = new Random();
        }

        public string GenerateOTP()
        {
            // Generate a 6-digit OTP
            return _random.Next(100000, 999999).ToString();
        }

        public bool ValidateOTP(string providedOTP, string storedOTP, DateTime? expiry)
        {
            if (string.IsNullOrEmpty(providedOTP) || string.IsNullOrEmpty(storedOTP))
                return false;

            if (expiry == null || DateTime.UtcNow > expiry)
                return false;

            return providedOTP.Equals(storedOTP, StringComparison.Ordinal);
        }

        public DateTime GetOTPExpiry()
        {
            // OTP expires in 10 minutes
            return DateTime.UtcNow.AddMinutes(10);
        }
    }
}