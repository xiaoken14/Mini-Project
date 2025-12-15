using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Appointment
    {
        [Key]
        public int Appointment_ID { get; set; }

        public int Admin_ID { get; set; }

        public int Doctor_ID { get; set; }

        public int Patient_ID { get; set; }

        public DateTime Appointment_Date { get; set; }

        public TimeSpan Appointment_Time { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(20)]
        public string? Priority { get; set; }

        [StringLength(10)]
        public string? Discount_Applied { get; set; }

        [ForeignKey("Admin_ID")]
        public virtual Admin? Admin { get; set; }

        [ForeignKey("Doctor_ID")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("Patient_ID")]
        public virtual Patient? Patient { get; set; }
    }
}
