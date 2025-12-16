namespace HealthcareApp.Services
{
    public interface IOTPService
    {
        string GenerateOTP();
        bool ValidateOTP(string providedOTP, string storedOTP, DateTime? expiry);
        DateTime GetOTPExpiry();
    }
}