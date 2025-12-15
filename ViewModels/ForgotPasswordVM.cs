using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
