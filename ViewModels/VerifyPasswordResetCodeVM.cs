using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class VerifyPasswordResetCodeVM
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Code { get; set; }
    }
}
