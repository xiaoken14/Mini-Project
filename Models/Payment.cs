using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Payment
    {
        [Key]
        public int Payment_ID { get; set; }

        [ForeignKey("Appointment")]
        public int Appointment_ID { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public DateTime Payment_Date { get; set; }

        [StringLength(20)]
        public string? Payment_Status { get; set; }

        [StringLength(20)]
        public string? Payment_Method { get; set; }

        public virtual Appointment? Appointment { get; set; }
    }
}
