using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        [Column("Notification_ID")]
        public int NotificationId { get; set; }

        [Column("Appointment_ID")]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Column("Message", TypeName = "TEXT")]
        public string? Message { get; set; }

        [Column("Notification_DateTime")]
        public DateTime NotificationDateTime { get; set; } = DateTime.UtcNow;

        [StringLength(30)]
        [Column("Type")]
        public string? Type { get; set; } // Confirmation / Reminder / Cancellation

        // Navigation property
        public virtual Appointment Appointment { get; set; } = null!;
    }
}