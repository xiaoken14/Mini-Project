using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Report
    {
        [Key]
        public int Report_ID { get; set; }

        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        public int Doctor_ID { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        public string Prescription { get; set; } = string.Empty;

        public DateTime Report_Date { get; set; }
    }
}
