using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsBodyHtml { get; set; } = false;

        public string? AttachmentPath { get; set; }
    }
}