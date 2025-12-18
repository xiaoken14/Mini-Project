using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareApp.Data;
using HealthcareApp.Models;
using HealthcareApp.Services;

namespace HealthcareApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScheduleService _scheduleService;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IScheduleService scheduleService)
        {
            _context = context;
            _userManager = userManager;
            _scheduleService = scheduleService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Schedule()
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var doctor = await _userManager.FindByIdAsync(doctorId);

                // Get schedules for this doctor using ApplicationUser ID
                var schedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .OrderBy(s => s.DayOfWeek)
                    .ToListAsync();

                var viewModel = new DoctorScheduleViewModel
                {
                    WeeklySchedule = schedules ?? new List<DoctorSchedule>(),
                    DoctorName = $"{doctor?.FirstName ?? "Doctor"} {doctor?.LastName ?? ""}".Trim(),
                    Specialization = doctor?.Specialization ?? "General Practice"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading the schedule: " + ex.Message;
                return View(new DoctorScheduleViewModel 
                { 
                    WeeklySchedule = new List<DoctorSchedule>(),
                    DoctorName = "Doctor",
                    Specialization = "General Practice"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSchedule(DoctorSchedule schedule)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                schedule.DoctorId = doctorId;
                schedule.UpdatedAt = DateTime.UtcNow;

                // Find existing schedule for this day
                var existingSchedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == schedule.DayOfWeek);

                if (existingSchedule != null)
                {
                    // Update existing schedule
                    existingSchedule.StartTime = schedule.StartTime;
                    existingSchedule.EndTime = schedule.EndTime;
                    existingSchedule.BreakStartTime = schedule.BreakStartTime;
                    existingSchedule.BreakEndTime = schedule.BreakEndTime;
                    existingSchedule.SlotDurationMinutes = schedule.SlotDurationMinutes;
                    existingSchedule.IsAvailable = schedule.IsAvailable;
                    existingSchedule.UpdatedAt = DateTime.UtcNow;
                    _context.Update(existingSchedule);
                }
                else
                {
                    // Create new schedule
                    schedule.CreatedAt = DateTime.UtcNow;
                    _context.DoctorSchedules.Add(schedule);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Schedule updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> DailyView(DateTime? date)
        {
            try
            {
                var selectedDate = date ?? DateTime.Today;
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var doctor = await _userManager.FindByIdAsync(doctorId);

                // Get the day schedule for the selected date
                var daySchedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == selectedDate.DayOfWeek);

                // Get all weekly schedules for the diagram
                var weeklySchedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .OrderBy(s => s.DayOfWeek)
                    .ToListAsync();

                // Get appointments for this doctor on the selected date
                // Note: This requires linking ApplicationUser.DoctorId to Doctor.DoctorId
                var appointments = new List<Appointment>();
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == doctorId);
                if (appUser?.DoctorId != null)
                {
                    appointments = await _context.Appointments
                        .Where(a => a.DoctorId == appUser.DoctorId.Value && 
                                    a.AppointmentDate.Date == selectedDate.Date)
                        .ToListAsync();
                }

                var viewModel = new DailyScheduleViewModel
                {
                    Date = selectedDate,
                    DayOfWeek = selectedDate.DayOfWeek,
                    DaySchedule = daySchedule,
                    WeeklySchedules = weeklySchedules ?? new List<DoctorSchedule>(),
                    DoctorName = $"{doctor?.FirstName ?? ""} {doctor?.LastName ?? ""}".Trim()
                };

                if (daySchedule != null && daySchedule.IsAvailable)
                {
                    try
                    {
                        viewModel.TimeSlots = GenerateTimeSlots(selectedDate, daySchedule, appointments);
                        viewModel.TotalSlots = viewModel.TimeSlots?.Count ?? 0;
                        viewModel.BookedSlots = viewModel.TimeSlots?.Count(s => s.IsBooked) ?? 0;
                        viewModel.AvailableSlots = viewModel.TimeSlots?.Count(s => s.IsAvailable && !s.IsBooked) ?? 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error generating time slots: {ex.Message}");
                        viewModel.TimeSlots = new List<TimeSlot>();
                    }
                }

                ViewBag.DoctorName = $"{doctor?.FirstName ?? ""} {doctor?.LastName ?? ""}".Trim();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading the daily schedule: " + ex.Message;
                return View(new DailyScheduleViewModel 
                { 
                    Date = date ?? DateTime.Today,
                    DayOfWeek = (date ?? DateTime.Today).DayOfWeek,
                    WeeklySchedules = new List<DoctorSchedule>(),
                    DoctorName = "Doctor",
                    TimeSlots = new List<TimeSlot>()
                });
            }
        }

        private List<TimeSlot> GenerateTimeSlots(DateTime date, DoctorSchedule schedule, List<Appointment> appointments)
        {
            var slots = new List<TimeSlot>();
            var currentTime = schedule.StartTime;
            var slotDuration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

            while (currentTime < schedule.EndTime)
            {
                var slotDateTime = date.Date.Add(currentTime);

                // Skip break time
                bool isBreakTime = schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue &&
                    currentTime >= schedule.BreakStartTime && currentTime < schedule.BreakEndTime;

                if (!isBreakTime)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentTime == currentTime);
                    slots.Add(new TimeSlot
                    {
                        DateTime = slotDateTime,
                        TimeString = currentTime.ToString(@"hh\:mm"),
                        IsAvailable = slotDateTime > DateTime.Now,
                        IsBooked = appointment != null,
                        AppointmentId = appointment?.AppointmentId.ToString()
                    });
                }

                currentTime = currentTime.Add(slotDuration);
            }

            return slots;
        }

        public IActionResult Patients()
        {
            return View();
        }

        public async Task<IActionResult> MonthlyView(DateTime? month)
        {
            try
            {
                var selectedMonth = month ?? DateTime.Today;
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = await _scheduleService.GetMonthlyScheduleAsync(doctorId, selectedMonth);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading the monthly schedule: " + ex.Message;
                return View(new MonthlyScheduleViewModel 
                { 
                    CurrentMonth = month ?? DateTime.Today,
                    DoctorName = "Doctor",
                    Specialization = "General Practice",
                    CalendarDays = new List<CalendarDay>(),
                    WeeklySchedules = new List<DoctorSchedule>(),
                    SpecialSchedules = new List<SpecialSchedule>(),
                    Stats = new MonthlyStats()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialSchedule(SpecialSchedule specialSchedule)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                specialSchedule.DoctorId = doctorId;

                // Handle time string conversion manually if needed
                if (Request.Form.ContainsKey("StartTime") && !string.IsNullOrEmpty(Request.Form["StartTime"]))
                {
                    if (TimeSpan.TryParse(Request.Form["StartTime"], out var startTime))
                        specialSchedule.StartTime = startTime;
                }

                if (Request.Form.ContainsKey("EndTime") && !string.IsNullOrEmpty(Request.Form["EndTime"]))
                {
                    if (TimeSpan.TryParse(Request.Form["EndTime"], out var endTime))
                        specialSchedule.EndTime = endTime;
                }

                if (Request.Form.ContainsKey("BreakStartTime") && !string.IsNullOrEmpty(Request.Form["BreakStartTime"]))
                {
                    if (TimeSpan.TryParse(Request.Form["BreakStartTime"], out var breakStartTime))
                        specialSchedule.BreakStartTime = breakStartTime;
                }

                if (Request.Form.ContainsKey("BreakEndTime") && !string.IsNullOrEmpty(Request.Form["BreakEndTime"]))
                {
                    if (TimeSpan.TryParse(Request.Form["BreakEndTime"], out var breakEndTime))
                        specialSchedule.BreakEndTime = breakEndTime;
                }

                // Remove validation errors for navigation properties and auto-set fields
                ModelState.Remove("Doctor");
                ModelState.Remove("DoctorId");
                ModelState.Remove("Id");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {string.Join(", ", x.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>())}")
                        .ToList();
                    return Json(new { success = false, message = "Validation failed: " + string.Join("; ", errors) });
                }

                var result = await _scheduleService.CreateSpecialScheduleAsync(specialSchedule);
                return Json(new { success = result, message = result ? "Special schedule created successfully!" : "Failed to create special schedule." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSpecialSchedule(SpecialSchedule specialSchedule)
        {
            var doctorId = _userManager.GetUserId(User);
            specialSchedule.DoctorId = doctorId!;

            var result = await _scheduleService.UpdateSpecialScheduleAsync(specialSchedule);
            return Json(new { success = result, message = result ? "Special schedule updated successfully!" : "Failed to update special schedule." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialSchedule(int id)
        {
            var doctorId = _userManager.GetUserId(User);
            var result = await _scheduleService.DeleteSpecialScheduleAsync(id, doctorId!);
            return Json(new { success = result, message = result ? "Special schedule deleted successfully!" : "Failed to delete special schedule." });
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateSchedule(BulkScheduleUpdateModel model)
        {
            var doctorId = _userManager.GetUserId(User);
            var result = await _scheduleService.BulkUpdateScheduleAsync(doctorId!, model);
            return Json(new { success = result, message = result ? "Bulk schedule update completed successfully!" : "Failed to update schedules." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTemplate(string templateName, string? description)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                if (string.IsNullOrEmpty(templateName))
                {
                    return Json(new { success = false, message = "Template name is required." });
                }

                var result = await _scheduleService.SaveScheduleTemplateAsync(doctorId, templateName, description);
                return Json(new { success = result, message = result ? "Schedule template saved successfully!" : "Failed to save template. Make sure you have schedules to save." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyTemplate(int templateId)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var result = await _scheduleService.ApplyScheduleTemplateAsync(doctorId, templateId);
                return Json(new { success = result, message = result ? "Template applied successfully!" : "Failed to apply template. Template may be corrupted or not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var templates = await _scheduleService.GetScheduleTemplatesAsync(doctorId);
                return Json(templates.Select(t => new { t.Id, t.Name, t.Description }));
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int dayOfWeek)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == (DayOfWeek)dayOfWeek);

                if (schedule != null)
                {
                    _context.DoctorSchedules.Remove(schedule);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Schedule deleted successfully!" });
                }

                return Json(new { success = false, message = "Schedule not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopySchedule(int sourceDayOfWeek, List<int> targetDays)
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var sourceSchedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == (DayOfWeek)sourceDayOfWeek);

                if (sourceSchedule == null)
                {
                    return Json(new { success = false, message = "Source schedule not found." });
                }

                foreach (var targetDay in targetDays)
                {
                    var existingSchedule = await _context.DoctorSchedules
                        .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == (DayOfWeek)targetDay);

                    if (existingSchedule != null)
                    {
                        // Update existing
                        existingSchedule.StartTime = sourceSchedule.StartTime;
                        existingSchedule.EndTime = sourceSchedule.EndTime;
                        existingSchedule.BreakStartTime = sourceSchedule.BreakStartTime;
                        existingSchedule.BreakEndTime = sourceSchedule.BreakEndTime;
                        existingSchedule.SlotDurationMinutes = sourceSchedule.SlotDurationMinutes;
                        existingSchedule.IsAvailable = sourceSchedule.IsAvailable;
                        existingSchedule.UpdatedAt = DateTime.UtcNow;
                        _context.Update(existingSchedule);
                    }
                    else
                    {
                        // Create new
                        var newSchedule = new DoctorSchedule
                        {
                            DoctorId = doctorId,
                            DayOfWeek = (DayOfWeek)targetDay,
                            StartTime = sourceSchedule.StartTime,
                            EndTime = sourceSchedule.EndTime,
                            BreakStartTime = sourceSchedule.BreakStartTime,
                            BreakEndTime = sourceSchedule.BreakEndTime,
                            SlotDurationMinutes = sourceSchedule.SlotDurationMinutes,
                            IsAvailable = sourceSchedule.IsAvailable,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.DoctorSchedules.Add(newSchedule);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Schedule copied to {targetDays.Count} day(s) successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public IActionResult Appointments()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllSchedules()
        {
            try
            {
                var doctorId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(doctorId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                // Remove all schedules for this doctor
                var schedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync();

                var specialSchedules = await _context.SpecialSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync();

                var templates = await _context.ScheduleTemplates
                    .Where(t => t.DoctorId == doctorId)
                    .ToListAsync();

                _context.DoctorSchedules.RemoveRange(schedules);
                _context.SpecialSchedules.RemoveRange(specialSchedules);
                _context.ScheduleTemplates.RemoveRange(templates);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Cleared {schedules.Count} schedules, {specialSchedules.Count} special schedules, and {templates.Count} templates." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
