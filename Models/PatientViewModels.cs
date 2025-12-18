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

    public class PaymentViewModel
    {
        [Required]
        public int AppointmentId { get; set; }
        
        [Required]
        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 13)]
        [RegularExpression(@"^[0-9\s]+$", ErrorMessage = "Card number can only contain numbers and spaces")]
        public string CardNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Cardholder Name")]
        [StringLength(100, MinimumLength = 2)]
        public string CardholderName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Expiry Month")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int ExpiryMonth { get; set; }
        
        [Required]
        [Display(Name = "Expiry Year")]
        [Range(2024, 2034, ErrorMessage = "Year must be between 2024 and 2034")]
        public int ExpiryYear { get; set; }
        
        [Required]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "CVV can only contain numbers")]
        public string CVV { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Amount")]
        [Range(0.01, 10000.00, ErrorMessage = "Amount must be between RM0.01 and RM10,000.00")]
        public decimal Amount { get; set; }
        
        // Discount properties
        public decimal OriginalAmount { get; set; }
        public decimal DiscountPercentage { get; set; } = 20.0m; // Fixed 20% discount
        public decimal DiscountAmount => OriginalAmount * (DiscountPercentage / 100);
        public decimal FinalAmount => OriginalAmount - DiscountAmount;
        
        // Display properties
        public Appointment? Appointment { get; set; }
    }

    public class PaymentHistoryViewModel
    {
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalDiscounts { get; set; }
    }

    public class PatientPaymentsViewModel
    {
        public List<Appointment> UnpaidAppointments { get; set; } = new();
        public List<Payment> PaymentHistory { get; set; } = new();
        public decimal TotalUnpaid { get; set; }
        public decimal TotalPaid { get; set; }
    }
}