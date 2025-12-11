using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Appointment
    {
        [Key]
        public int Appointment_ID { get; set; }

        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        public int Doctor_ID { get; set; }
        public Doctor? Doctor { get; set; }

        public DateTime Appointment_Date { get; set; }

        [StringLength(50)]
        public string Slot { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;
    }
}
