using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class UpdatePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = "";
    }
}
