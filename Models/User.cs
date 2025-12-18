using Microsoft.AspNetCore.Identity;

namespace HealthcareApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Doctor specific properties
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        
        // Patient specific properties
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        
        // OTP verification properties
        public string? EmailOTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public int OTPAttempts { get; set; } = 0;
        
        // Additional tracking properties
        public DateTime? LastLoginTime { get; set; }
        
        // Link to legacy Doctor table (nullable for non-doctors)
        public int? DoctorId { get; set; }
        
        // Link to legacy Patient table (nullable for non-patients)
        public int? PatientId { get; set; }
    }

    public enum UserRole
    {
        Patient = 1,
        Doctor = 2,
        Admin = 3
    }
}