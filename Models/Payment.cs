using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [Column("Payment_ID")]
        public int PaymentId { get; set; }

        [Column("Appointment_ID")]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Column("Amount", TypeName = "DECIMAL(10,2)")]
        public decimal Amount { get; set; }

        [Column("Payment_Date")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        [Column("Payment_Status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending / Completed / Failed

        [StringLength(20)]
        [Column("Payment_Method")]
        public string? PaymentMethod { get; set; } // Cash / Online

        // Navigation property
        public virtual Appointment Appointment { get; set; } = null!;
    }
}