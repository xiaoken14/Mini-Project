using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; } = string.Empty;
        public ApplicationUser Patient { get; set; } = null!;
        
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        public ApplicationUser Doctor { get; set; } = null!;
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum AppointmentStatus
    {
        Pending = 1,
        Confirmed = 2,
        Completed = 3,
        Cancelled = 4,
        Rescheduled = 5
    }
}