using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Notification
    {
        [Key]
        public int Notification_ID { get; set; }

        public int User_ID { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool Is_Read { get; set; }
    }
}
