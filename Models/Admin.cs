using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Admin
    {
        [Key]
        public int Admin_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Full_Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [StringLength(20)]
        public string? Contact_No { get; set; }

        public DateTime Date_Created { get; set; } = DateTime.UtcNow;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
