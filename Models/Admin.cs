using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Admin")]
    public class Admin
    {
        [Key]
        [Column("Admin_ID")]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Full_Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("Contact_No")]
        public string? ContactNo { get; set; }

        [Column("Date_Created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}