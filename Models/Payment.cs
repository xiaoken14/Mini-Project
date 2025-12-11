using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Payment
    {
        [Key]
        public int Payment_ID { get; set; }

        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        public int Appointment_ID { get; set; }
        public Appointment? Appointment { get; set; }

        public decimal Amount { get; set; }

        public DateTime Payment_Date { get; set; }

        [StringLength(20)]
        public string Payment_Status { get; set; } = string.Empty;

        [StringLength(50)]
        public string Payment_Method { get; set; } = string.Empty;
    }
}
