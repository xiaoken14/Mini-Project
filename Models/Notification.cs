using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Notification
    {
        [Key]
        public int Notification_ID { get; set; }

        [ForeignKey("Appointment")]
        public int Appointment_ID { get; set; }

        public string? Message { get; set; }

        public DateTime Notification_DateTime { get; set; }

        [StringLength(30)]
        public string? Type { get; set; }

        public virtual Appointment? Appointment { get; set; }
    }
}
