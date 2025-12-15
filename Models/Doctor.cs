using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Doctor
    {
        [Key]
        public int Doctor_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Full_Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(50)]
        public string? Consultation_Hours { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        public int Admin_ID { get; set; }

        [ForeignKey("Admin_ID")]
        public Admin? Admin { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
