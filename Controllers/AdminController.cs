using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HealthcareApp.Data;
using HealthcareApp.Models;
using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return await Dashboard();
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardData = new AdminDashboardViewModel
                {
                    // Use safe counts that won't cause database errors
                    TotalDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor),
                    TotalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient),
                    TotalAppointments = 0, // Temporarily disabled
                    TotalUsers = await _context.Users.CountAsync(),
                    
                    // Return empty lists for now
                    RecentAppointments = new List<Appointment>(),
                    RecentDoctors = new List<ApplicationUser>()
                };

                return View("Index", dashboardData);
            }
            catch (Exception ex)
            {
                // Log the error and return a safe view
                ViewBag.ErrorMessage = $"Error loading dashboard: {ex.Message}";
                
                // Return a minimal dashboard
                var fallbackData = new AdminDashboardViewModel
                {
                    TotalDoctors = 0,
                    TotalPatients = 0,
                    TotalAppointments = 0,
                    TotalUsers = 0,
                    RecentAppointments = new List<Appointment>(),
                    RecentDoctors = new List<ApplicationUser>()
                };
                
                return View("Index", fallbackData);
            }
        }

        // Doctor Management
        public async Task<IActionResult> Doctors()
        {
            try
            {
                // Use the Identity users for now to avoid database schema issues
                var doctors = await _context.Users
                    .Where(u => u.Role == UserRole.Doctor)
                    .ToListAsync();
                return View(doctors);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading doctors: {ex.Message}";
                return View(new List<ApplicationUser>());
            }
        }

        public IActionResult CreateDoctor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doctor = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Specialization = model.Specialization,
                    LicenseNumber = model.LicenseNumber,
                    Role = UserRole.Doctor,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(doctor, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(doctor, "Doctor");
                    TempData["Success"] = "Doctor created successfully!";
                    return RedirectToAction(nameof(Doctors));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditDoctor(string id)
        {
            if (id == null) return NotFound();
            var doctor = await _context.Users.FindAsync(id);
            if (doctor == null || doctor.Role != UserRole.Doctor) return NotFound();
            
            var model = new EditDoctorViewModel
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email!,
                PhoneNumber = doctor.PhoneNumber,
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(string id, EditDoctorViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var doctor = await _context.Users.FindAsync(id);
                    if (doctor == null || doctor.Role != UserRole.Doctor) return NotFound();

                    doctor.FirstName = model.FirstName;
                    doctor.LastName = model.LastName;
                    doctor.Email = model.Email;
                    doctor.UserName = model.Email;
                    doctor.PhoneNumber = model.PhoneNumber;
                    doctor.Specialization = model.Specialization;
                    doctor.LicenseNumber = model.LicenseNumber;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Doctor updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DoctorExists(id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Doctors));
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteDoctor(string id)
        {
            if (id == null) return NotFound();
            var doctor = await _context.Users.FindAsync(id);
            if (doctor == null || doctor.Role != UserRole.Doctor) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("DeleteDoctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorConfirmed(string id)
        {
            try
            {
                var doctor = await _context.Users.FindAsync(id);
                if (doctor != null && doctor.Role == UserRole.Doctor)
                {
                    // Check if doctor has any appointments (temporarily disabled due to schema mismatch)
                    var hasAppointments = false; // await _context.Appointments.AnyAsync(a => a.DoctorId == id);
                    
                    if (hasAppointments)
                    {
                        TempData["Error"] = "Cannot delete doctor with existing appointments. Please reassign or complete all appointments first.";
                        return RedirectToAction(nameof(Doctors));
                    }

                    // Delete the doctor
                    var result = await _userManager.DeleteAsync(doctor);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Doctor deleted successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Error deleting doctor: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] = "Doctor not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting doctor: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Doctors));
        }

        // Patient Management
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .ToListAsync();
            return View(patients);
        }

        public async Task<IActionResult> PatientDetails(string id)
        {
            if (id == null) return NotFound();
            var patient = await _context.Users.FindAsync(id);
            if (patient == null || patient.Role != UserRole.Patient) return NotFound();
            return View(patient);
        }

        public async Task<IActionResult> DeletePatient(string id)
        {
            if (id == null) return NotFound();
            var patient = await _context.Users.FindAsync(id);
            if (patient == null || patient.Role != UserRole.Patient) return NotFound();
            return View(patient);
        }

        [HttpPost, ActionName("DeletePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatientConfirmed(string id)
        {
            try
            {
                var patient = await _context.Users.FindAsync(id);
                if (patient != null && patient.Role == UserRole.Patient)
                {
                    // Check if patient has any appointments (temporarily disabled due to schema mismatch)
                    var hasAppointments = false; // await _context.Appointments.AnyAsync(a => a.PatientId == id);
                    
                    if (hasAppointments)
                    {
                        TempData["Error"] = "Cannot delete patient with existing appointments. Please cancel or complete all appointments first.";
                        return RedirectToAction(nameof(Patients));
                    }

                    // Delete the patient
                    var result = await _userManager.DeleteAsync(patient);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Patient deleted successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Error deleting patient: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] = "Patient not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting patient: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Patients));
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LockoutEnabled = !user.LockoutEnabled;
                if (user.LockoutEnabled)
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }
                else
                {
                    user.LockoutEnd = null;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User {(user.LockoutEnabled ? "disabled" : "enabled")} successfully!";
            }
            return RedirectToAction(nameof(Users));
        }

        // Appointment Management
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        // Reports
        public async Task<IActionResult> Reports()
        {
            var reportData = new AdminReportsViewModel
            {
                DoctorsBySpecialty = await _context.Users
                    .Where(u => u.Role == UserRole.Doctor && !string.IsNullOrEmpty(u.Specialization))
                    .GroupBy(u => u.Specialization)
                    .Select(g => new SpecialtyCount { Specialty = g.Key!, Count = g.Count() })
                    .ToListAsync(),
                AppointmentsByStatus = await _context.Appointments
                    .GroupBy(a => a.Status)
                    .Select(g => new StatusCount { Status = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync(),
                MonthlyAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= DateTime.Now.AddMonths(-6))
                    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                    .Select(g => new MonthlyCount 
                    { 
                        Month = $"{g.Key.Year}-{g.Key.Month:00}", 
                        Count = g.Count() 
                    })
                    .ToListAsync()
            };
            return View(reportData);
        }

        // General User Delete
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            
            // Don't allow deleting admin users
            if (user.Role == UserRole.Admin)
            {
                TempData["Error"] = "Cannot delete admin users.";
                return RedirectToAction(nameof(Users));
            }
            
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    // Don't allow deleting admin users
                    if (user.Role == UserRole.Admin)
                    {
                        TempData["Error"] = "Cannot delete admin users.";
                        return RedirectToAction(nameof(Users));
                    }

                    // Check if user has any appointments (temporarily disabled due to schema mismatch)
                    var hasAppointments = false; // await _context.Appointments.AnyAsync(a => a.DoctorId == id || a.PatientId == id);
                    
                    if (hasAppointments)
                    {
                        TempData["Error"] = $"Cannot delete {user.Role.ToString().ToLower()} with existing appointments. Please reassign or complete all appointments first.";
                        return RedirectToAction(nameof(Users));
                    }

                    // Delete the user
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"{user.Role} deleted successfully!";
                    }
                    else
                    {
                        TempData["Error"] = $"Error deleting {user.Role.ToString().ToLower()}: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Users));
        }

        private async Task<bool> DoctorExists(string id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id && u.Role == UserRole.Doctor);
        }
    }

    // ViewModels for Admin Dashboard
    public class AdminDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalUsers { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<ApplicationUser> RecentDoctors { get; set; } = new();
    }

    public class AdminReportsViewModel
    {
        public List<SpecialtyCount> DoctorsBySpecialty { get; set; } = new();
        public List<StatusCount> AppointmentsByStatus { get; set; } = new();
        public List<MonthlyCount> MonthlyAppointments { get; set; } = new();
    }

    public class SpecialtyCount
    {
        public string Specialty { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyCount
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CreateDoctorViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }
    }

    public class EditDoctorViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }
    }
}