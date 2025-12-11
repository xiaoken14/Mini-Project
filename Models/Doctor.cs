using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Doctor
    {
        [Key]
        public int Doctor_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Full_Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(50)]
        public string Consultation_Hours { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public int Admin_ID { get; set; }
        public Admin? Admin { get; set; }
    }
}
