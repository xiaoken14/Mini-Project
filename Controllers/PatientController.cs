using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareApp.Data;
using HealthcareApp.Models;

namespace HealthcareApp.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return await Dashboard();
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Initialize empty lists in case of database issues
                var upcomingAppointments = new List<Appointment>();
                var recentAppointments = new List<Appointment>();
                var totalAppointments = 0;
                var pendingAppointments = 0;

                try
                {
                    upcomingAppointments = await _context.Appointments
                        .Include(a => a.Doctor)
                        .Where(a => a.PatientId == user.Id && a.AppointmentDate >= DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .ThenBy(a => a.AppointmentTime.Ticks)
                        .Take(5)
                        .ToListAsync();

                    recentAppointments = await _context.Appointments
                        .Include(a => a.Doctor)
                        .Where(a => a.PatientId == user.Id && a.AppointmentDate < DateTime.Today)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.AppointmentTime.Ticks)
                        .Take(5)
                        .ToListAsync();

                    totalAppointments = await _context.Appointments
                        .CountAsync(a => a.PatientId == user.Id);

                    pendingAppointments = await _context.Appointments
                        .CountAsync(a => a.PatientId == user.Id && a.Status == AppointmentStatus.Pending);
                }
                catch (Exception ex)
                {
                    // Log the exception (in a real app, use proper logging)
                    Console.WriteLine($"Database error in Dashboard: {ex.Message}");
                    // Continue with empty data
                }

                var viewModel = new PatientDashboardViewModel
                {
                    Patient = user,
                    UpcomingAppointments = upcomingAppointments,
                    RecentAppointments = recentAppointments,
                    TotalAppointments = totalAppointments,
                    PendingAppointments = pendingAppointments
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                Console.WriteLine($"Error in Dashboard: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Appointments()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == user.Id)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime.Ticks)
                    .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                Console.WriteLine($"Error in Appointments: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return empty list to avoid complete failure
                return View(new List<Appointment>());
            }
        }

        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            
            var viewModel = new BookAppointmentViewModel
            {
                AvailableDoctors = doctors.ToList(),
                AppointmentDate = DateTime.Today.AddDays(1)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if the selected date and time is in the future
                var appointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);
                if (appointmentDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("", "Appointment date and time must be in the future.");
                }
                else
                {
                    // Check if doctor is available at this time
                    var existingAppointment = await _context.Appointments
                        .AnyAsync(a => a.DoctorId == model.DoctorId && 
                                      a.AppointmentDate == model.AppointmentDate && 
                                      a.AppointmentTime == model.AppointmentTime &&
                                      a.Status != AppointmentStatus.Cancelled);

                    if (existingAppointment)
                    {
                        ModelState.AddModelError("", "The selected time slot is not available. Please choose a different time.");
                    }
                    else
                    {
                        var appointment = new Appointment
                        {
                            PatientId = user.Id,
                            DoctorId = model.DoctorId,
                            AppointmentDate = model.AppointmentDate,
                            AppointmentTime = model.AppointmentTime,
                            Reason = model.Reason,
                            Status = AppointmentStatus.Pending
                        };

                        _context.Appointments.Add(appointment);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Appointment booked successfully! You will receive a confirmation email shortly.";
                        return RedirectToAction(nameof(Appointments));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            model.AvailableDoctors = doctors.ToList();
            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new PatientProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                EmergencyContact = user.EmergencyContact
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(PatientProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;
                user.Address = model.Address;
                user.EmergencyContact = model.EmergencyContact;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.AppointmentDate <= DateTime.Today)
            {
                TempData["ErrorMessage"] = "Cannot cancel appointments on the same day or past appointments.";
                return RedirectToAction(nameof(Appointments));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(Appointments));
        }
    }
}