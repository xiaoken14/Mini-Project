using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Doctor")]
    public class Doctor
    {
        [Key]
        [Column("Doctor_ID")]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Full_Name")]
        public string FullName { get; set; } = string.Empty;

        // Add compatibility properties for existing views
        public string FirstName 
        { 
            get 
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : string.Empty;
            }
        }
        
        public string LastName 
        { 
            get 
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
            }
        }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("Specialization")]
        public string? Specialization { get; set; }

        [StringLength(50)]
        [Column("Consultation_Hours")]
        public string? ConsultationHours { get; set; }

        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Active";

        [Column("Admin_ID")]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        // Navigation properties
        public virtual Admin Admin { get; set; } = null!;
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}