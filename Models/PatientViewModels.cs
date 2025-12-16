using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class BookAppointmentViewModel
    {
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        [Display(Name = "Reason for Visit")]
        public string Reason { get; set; } = string.Empty;
        
        public List<ApplicationUser> AvailableDoctors { get; set; } = new();
    }

    public class PatientProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [Display(Name = "Address")]
        [StringLength(500)]
        public string? Address { get; set; }
        
        [Display(Name = "Emergency Contact")]
        [StringLength(200)]
        public string? EmergencyContact { get; set; }
    }

    public class PatientDashboardViewModel
    {
        public ApplicationUser Patient { get; set; } = null!;
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
    }
}