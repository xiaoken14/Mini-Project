using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Patient
    {
        [Key]
        public int Patient_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Full_Name { get; set; } = "";

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = "";

        [StringLength(20)]
        public string? Contact_No { get; set; }

        public int Age { get; set; }

        [StringLength(20)]
        public string? Category { get; set; }

        [StringLength(10)]
        public string? Discount_Eligibility { get; set; }

        // Must be nullable for self-registration
        public int? Admin_ID { get; set; }

        [ForeignKey("Admin_ID")]
        public virtual Admin? Admin { get; set; }
        
        // This helper property is not in the database table, but needed for the Controller logic
        public string? EmailConfirmationToken { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}