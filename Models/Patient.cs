using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Patient
    {
        [Key]
        public int Patient_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Full_Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Contact_No { get; set; } = string.Empty;

        public DateTime Date_of_Birth { get; set; }

        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        public string Medical_History { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public int Admin_ID { get; set; }
        public Admin? Admin { get; set; }

        // Foreign key for the IdentityUser
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}
