using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class EmailVerificationVM
    {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string VerificationCode { get; set; } = "";
    }
}
