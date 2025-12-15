using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class ResetPasswordConfirmVM
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Code { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmPassword { get; set; }
    }
}
