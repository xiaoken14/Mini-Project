using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Appointment")]
    public class Appointment
    {
        [Key]
        [Column("Appointment_ID")]
        public int AppointmentId { get; set; }

        // Add compatibility property for existing code
        public int Id => AppointmentId;

        [Column("Admin_ID")]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        [Column("Doctor_ID")]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Column("Patient_ID")]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [Column("Appointment_Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Column("Appointment_Time")]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Booked"; // Booked / Cancelled / Completed

        [StringLength(20)]
        [Column("Priority")]
        public string Priority { get; set; } = "Normal"; // Normal / Student / Senior

        [StringLength(10)]
        [Column("Discount_Applied")]
        public string DiscountApplied { get; set; } = "No"; // Yes / No

        // Add compatibility properties for existing views
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Admin Admin { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}