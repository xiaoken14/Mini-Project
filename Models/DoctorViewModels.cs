namespace HealthcareApp.Models
{
    public class DoctorScheduleViewModel
    {
        public List<DoctorSchedule> WeeklySchedule { get; set; } = new();
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }

    public class DailyScheduleViewModel
    {
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DoctorSchedule? DaySchedule { get; set; }
        public List<TimeSlot> TimeSlots { get; set; } = new();
        public int TotalSlots { get; set; }
        public int BookedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public List<DoctorSchedule> WeeklySchedules { get; set; } = new();
        public string DoctorName { get; set; } = string.Empty;
    }

    public class TimeSlot
    {
        public DateTime DateTime { get; set; }
        public string TimeString { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsBooked { get; set; }
        public string? AppointmentId { get; set; }
    }

    public class MonthlyScheduleViewModel
    {
        public DateTime CurrentMonth { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public List<CalendarDay> CalendarDays { get; set; } = new();
        public List<DoctorSchedule> WeeklySchedules { get; set; } = new();
        public List<SpecialSchedule> SpecialSchedules { get; set; } = new();
        public MonthlyStats Stats { get; set; } = new();
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasSchedule { get; set; }
        public bool HasSpecialSchedule { get; set; }
        public int AppointmentCount { get; set; }
        public string CssClass { get; set; } = string.Empty;
    }

    public class MonthlyStats
    {
        public int TotalWorkingDays { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalAvailableSlots { get; set; }
        public double BookingRate { get; set; }
    }

    public class BulkScheduleUpdateModel
    {
        public List<int> DaysOfWeek { get; set; } = new();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsAvailable { get; set; } = true;
    }
}