using HealthcareApp.Data;
using HealthcareApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApp.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyScheduleViewModel> GetMonthlyScheduleAsync(string doctorId, DateTime month)
        {
            var doctor = await _context.Users.FindAsync(doctorId);
            var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var weeklySchedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();

            var specialSchedules = await _context.SpecialSchedules
                .Where(s => s.DoctorId == doctorId && s.Date >= firstDayOfMonth && s.Date <= lastDayOfMonth)
                .ToListAsync();

            // Get appointments for this doctor
            var appointments = new List<Appointment>();
            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == doctorId);
            if (appUser?.DoctorId != null)
            {
                appointments = await _context.Appointments
                    .Where(a => a.DoctorId == appUser.DoctorId.Value && 
                                a.AppointmentDate >= firstDayOfMonth && 
                                a.AppointmentDate <= lastDayOfMonth)
                    .ToListAsync();
            }

            var calendarDays = GenerateCalendarDays(month, weeklySchedules, specialSchedules, appointments);
            var stats = CalculateMonthlyStats(weeklySchedules, specialSchedules, appointments, month);

            return new MonthlyScheduleViewModel
            {
                CurrentMonth = month,
                DoctorName = $"{doctor?.FirstName} {doctor?.LastName}",
                Specialization = doctor?.Specialization ?? "General Practice",
                CalendarDays = calendarDays,
                WeeklySchedules = weeklySchedules,
                SpecialSchedules = specialSchedules,
                Stats = stats
            };
        }

        public async Task<bool> CreateSpecialScheduleAsync(SpecialSchedule specialSchedule)
        {
            try
            {
                specialSchedule.CreatedAt = DateTime.UtcNow;
                specialSchedule.UpdatedAt = DateTime.UtcNow;
                _context.SpecialSchedules.Add(specialSchedule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSpecialScheduleAsync(SpecialSchedule specialSchedule)
        {
            try
            {
                var existing = await _context.SpecialSchedules.FindAsync(specialSchedule.Id);
                if (existing == null || existing.DoctorId != specialSchedule.DoctorId)
                    return false;

                existing.Date = specialSchedule.Date;
                existing.Type = specialSchedule.Type;
                existing.StartTime = specialSchedule.StartTime;
                existing.EndTime = specialSchedule.EndTime;
                existing.BreakStartTime = specialSchedule.BreakStartTime;
                existing.BreakEndTime = specialSchedule.BreakEndTime;
                existing.SlotDurationMinutes = specialSchedule.SlotDurationMinutes;
                existing.Note = specialSchedule.Note;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSpecialScheduleAsync(int id, string doctorId)
        {
            try
            {
                var specialSchedule = await _context.SpecialSchedules
                    .FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctorId);
                
                if (specialSchedule == null)
                    return false;

                _context.SpecialSchedules.Remove(specialSchedule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BulkUpdateScheduleAsync(string doctorId, BulkScheduleUpdateModel model)
        {
            try
            {
                foreach (var dayOfWeek in model.DaysOfWeek)
                {
                    var existing = await _context.DoctorSchedules
                        .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == (DayOfWeek)dayOfWeek);

                    if (existing != null)
                    {
                        existing.StartTime = model.StartTime;
                        existing.EndTime = model.EndTime;
                        existing.BreakStartTime = model.BreakStartTime;
                        existing.BreakEndTime = model.BreakEndTime;
                        existing.SlotDurationMinutes = model.SlotDurationMinutes;
                        existing.IsAvailable = model.IsAvailable;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newSchedule = new DoctorSchedule
                        {
                            DoctorId = doctorId,
                            DayOfWeek = (DayOfWeek)dayOfWeek,
                            StartTime = model.StartTime,
                            EndTime = model.EndTime,
                            BreakStartTime = model.BreakStartTime,
                            BreakEndTime = model.BreakEndTime,
                            SlotDurationMinutes = model.SlotDurationMinutes,
                            IsAvailable = model.IsAvailable,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.DoctorSchedules.Add(newSchedule);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveScheduleTemplateAsync(string doctorId, string templateName, string? description)
        {
            try
            {
                var currentSchedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync();

                if (!currentSchedules.Any())
                {
                    return false; // No schedules to save as template
                }

                // Store template data as JSON in description (workaround)
                var templateData = currentSchedules.Select(s => new
                {
                    DayOfWeek = (int)s.DayOfWeek,
                    StartTime = s.StartTime.ToString(),
                    EndTime = s.EndTime.ToString(),
                    BreakStartTime = s.BreakStartTime?.ToString(),
                    BreakEndTime = s.BreakEndTime?.ToString(),
                    SlotDurationMinutes = s.SlotDurationMinutes,
                    IsAvailable = s.IsAvailable
                }).ToList();

                var templateJson = System.Text.Json.JsonSerializer.Serialize(templateData);

                var template = new ScheduleTemplate
                {
                    DoctorId = doctorId,
                    Name = templateName,
                    Description = $"{description ?? ""}\n__TEMPLATE_DATA__:{templateJson}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ScheduleTemplates.Add(template);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving template: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApplyScheduleTemplateAsync(string doctorId, int templateId)
        {
            try
            {
                var template = await _context.ScheduleTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.DoctorId == doctorId);

                if (template == null || string.IsNullOrEmpty(template.Description))
                    return false;

                // Extract template data from description (workaround)
                var templateDataStart = template.Description.IndexOf("__TEMPLATE_DATA__:");
                if (templateDataStart == -1)
                    return false;

                var templateJson = template.Description.Substring(templateDataStart + "__TEMPLATE_DATA__:".Length);
                var templateData = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(templateJson);

                if (templateData == null)
                    return false;

                // Remove existing schedules
                var existingSchedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync();
                _context.DoctorSchedules.RemoveRange(existingSchedules);

                // Apply template schedules
                foreach (var item in templateData)
                {
                    var jsonElement = (System.Text.Json.JsonElement)item;
                    var newSchedule = new DoctorSchedule
                    {
                        DoctorId = doctorId,
                        DayOfWeek = (DayOfWeek)jsonElement.GetProperty("DayOfWeek").GetInt32(),
                        StartTime = TimeSpan.Parse(jsonElement.GetProperty("StartTime").GetString() ?? "09:00:00"),
                        EndTime = TimeSpan.Parse(jsonElement.GetProperty("EndTime").GetString() ?? "17:00:00"),
                        BreakStartTime = jsonElement.TryGetProperty("BreakStartTime", out var breakStart) && 
                                        !breakStart.ValueKind.Equals(System.Text.Json.JsonValueKind.Null) 
                                        ? TimeSpan.Parse(breakStart.GetString() ?? "12:00:00") : null,
                        BreakEndTime = jsonElement.TryGetProperty("BreakEndTime", out var breakEnd) && 
                                      !breakEnd.ValueKind.Equals(System.Text.Json.JsonValueKind.Null) 
                                      ? TimeSpan.Parse(breakEnd.GetString() ?? "13:00:00") : null,
                        SlotDurationMinutes = jsonElement.GetProperty("SlotDurationMinutes").GetInt32(),
                        IsAvailable = jsonElement.GetProperty("IsAvailable").GetBoolean(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.DoctorSchedules.Add(newSchedule);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying template: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ScheduleTemplate>> GetScheduleTemplatesAsync(string doctorId)
        {
            var templates = await _context.ScheduleTemplates
                .Where(t => t.DoctorId == doctorId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            // Clean up descriptions for display (remove template data)
            foreach (var template in templates)
            {
                if (!string.IsNullOrEmpty(template.Description))
                {
                    var templateDataStart = template.Description.IndexOf("__TEMPLATE_DATA__:");
                    if (templateDataStart > 0)
                    {
                        template.Description = template.Description.Substring(0, templateDataStart).Trim();
                    }
                }
            }

            return templates;
        }

        private List<CalendarDay> GenerateCalendarDays(DateTime month, List<DoctorSchedule> weeklySchedules, 
            List<SpecialSchedule> specialSchedules, List<Appointment> appointments)
        {
            var days = new List<CalendarDay>();
            var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
            var endDate = lastDayOfMonth.AddDays(6 - (int)lastDayOfMonth.DayOfWeek);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var hasSchedule = weeklySchedules.Any(s => s.DayOfWeek == date.DayOfWeek && s.IsAvailable);
                var hasSpecialSchedule = specialSchedules.Any(s => s.Date.Date == date.Date);
                var appointmentCount = appointments.Count(a => a.AppointmentDate.Date == date.Date);

                days.Add(new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == month.Month,
                    IsToday = date.Date == DateTime.Today,
                    HasSchedule = hasSchedule,
                    HasSpecialSchedule = hasSpecialSchedule,
                    AppointmentCount = appointmentCount,
                    CssClass = GetCalendarDayCssClass(date, month, hasSchedule, hasSpecialSchedule, appointmentCount)
                });
            }

            return days;
        }

        private string GetCalendarDayCssClass(DateTime date, DateTime month, bool hasSchedule, 
            bool hasSpecialSchedule, int appointmentCount)
        {
            var classes = new List<string>();

            if (date.Month != month.Month)
                classes.Add("other-month");
            
            if (date.Date == DateTime.Today)
                classes.Add("today");
            
            if (hasSpecialSchedule)
                classes.Add("special-schedule");
            else if (hasSchedule)
                classes.Add("has-schedule");
            
            if (appointmentCount > 0)
                classes.Add("has-appointments");

            return string.Join(" ", classes);
        }

        private MonthlyStats CalculateMonthlyStats(List<DoctorSchedule> weeklySchedules, 
            List<SpecialSchedule> specialSchedules, List<Appointment> appointments, DateTime month)
        {
            var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var workingDays = 0;
            var totalSlots = 0;

            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var daySchedule = weeklySchedules.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek && s.IsAvailable);
                var specialSchedule = specialSchedules.FirstOrDefault(s => s.Date.Date == date.Date);

                if (daySchedule != null || specialSchedule != null)
                {
                    workingDays++;
                    
                    // Calculate slots for the day
                    TimeSpan? startTime = null;
                    TimeSpan? endTime = null;
                    TimeSpan? breakStartTime = null;
                    TimeSpan? breakEndTime = null;
                    int slotDurationMinutes = 30;

                    if (specialSchedule != null)
                    {
                        startTime = specialSchedule.StartTime;
                        endTime = specialSchedule.EndTime;
                        breakStartTime = specialSchedule.BreakStartTime;
                        breakEndTime = specialSchedule.BreakEndTime;
                        slotDurationMinutes = specialSchedule.SlotDurationMinutes;
                    }
                    else if (daySchedule != null)
                    {
                        startTime = daySchedule.StartTime;
                        endTime = daySchedule.EndTime;
                        breakStartTime = daySchedule.BreakStartTime;
                        breakEndTime = daySchedule.BreakEndTime;
                        slotDurationMinutes = daySchedule.SlotDurationMinutes;
                    }

                    if (startTime.HasValue && endTime.HasValue && startTime < endTime)
                    {
                        var duration = endTime.Value - startTime.Value;
                        var breakDuration = TimeSpan.Zero;
                        
                        if (breakStartTime.HasValue && breakEndTime.HasValue)
                            breakDuration = breakEndTime.Value - breakStartTime.Value;
                        
                        var workingDuration = duration - breakDuration;
                        var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);
                        
                        if (slotDuration > TimeSpan.Zero)
                            totalSlots += (int)(workingDuration.TotalMinutes / slotDuration.TotalMinutes);
                    }
                }
            }

            var totalAppointments = appointments.Count;
            var bookingRate = totalSlots > 0 ? (double)totalAppointments / totalSlots * 100 : 0;

            return new MonthlyStats
            {
                TotalWorkingDays = workingDays,
                TotalAppointments = totalAppointments,
                TotalAvailableSlots = totalSlots,
                BookingRate = Math.Round(bookingRate, 1)
            };
        }
    }
}