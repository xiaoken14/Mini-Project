using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }
        
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        
        public ApplicationUser? Doctor { get; set; }
        
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Required]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }
        
        [Display(Name = "Break Start")]
        public TimeSpan? BreakStartTime { get; set; }
        
        [Display(Name = "Break End")]
        public TimeSpan? BreakEndTime { get; set; }
        
        [Range(15, 120)]
        [Display(Name = "Slot Duration (minutes)")]
        public int SlotDurationMinutes { get; set; } = 30;
        
        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;
        
        public int? ScheduleTemplateId { get; set; }
        
        public ScheduleTemplate? ScheduleTemplate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SpecialSchedule
    {
        public int Id { get; set; }
        
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        
        public ApplicationUser? Doctor { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public SpecialScheduleType Type { get; set; }
        
        public TimeSpan? StartTime { get; set; }
        
        public TimeSpan? EndTime { get; set; }
        
        public TimeSpan? BreakStartTime { get; set; }
        
        public TimeSpan? BreakEndTime { get; set; }
        
        [Range(15, 120)]
        public int SlotDurationMinutes { get; set; } = 30;
        
        public string? Note { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum SpecialScheduleType
    {
        Holiday,
        Vacation,
        Conference,
        Emergency,
        CustomHours
    }

    public class ScheduleTemplate
    {
        public int Id { get; set; }
        
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        
        public ApplicationUser? Doctor { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public List<DoctorSchedule> WeeklySchedules { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}