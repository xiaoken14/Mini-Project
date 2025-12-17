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
                    // For now, return empty lists since we need to implement proper user mapping
                    // This will need to be updated when we implement the new authentication system
                    upcomingAppointments = new List<Appointment>();
                    recentAppointments = new List<Appointment>();
                    totalAppointments = 0;
                    pendingAppointments = 0;
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

                // For now, return empty list since we need to implement proper user mapping
                var appointments = new List<Appointment>();

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
                    // For now, just show an error since we need to implement proper user mapping
                    ModelState.AddModelError("", "Appointment booking is temporarily unavailable. Please contact support.");
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

            // For now, return null since we need to implement proper user mapping
            Appointment? appointment = null;

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.AppointmentDate <= DateTime.Today)
            {
                TempData["ErrorMessage"] = "Cannot cancel appointments on the same day or past appointments.";
                return RedirectToAction(nameof(Appointments));
            }

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(Appointments));
        }
    }
}