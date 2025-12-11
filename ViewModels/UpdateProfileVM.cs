using System.ComponentModel.DataAnnotations;

namespace Mini_Project.ViewModels
{
    public class UpdateProfileVM
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
