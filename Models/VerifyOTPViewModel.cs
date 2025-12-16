using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class VerifyOTPViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers")]
        public string OTP { get; set; } = string.Empty;
    }
}