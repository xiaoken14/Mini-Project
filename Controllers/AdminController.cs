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
            var dashboardData = new AdminDashboardViewModel
            {
                TotalDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor),
                TotalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),
                RecentDoctors = await _context.Users
                    .Where(u => u.Role == UserRole.Doctor)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardData);
        }

        // Doctor Management
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Users
                .Where(u => u.Role == UserRole.Doctor)
                .ToListAsync();
            return View(doctors);
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
            var doctor = await _context.Users.FindAsync(id);
            if (doctor != null && doctor.Role == UserRole.Doctor)
            {
                _context.Users.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor deleted successfully!";
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

        // System Settings
        public IActionResult Settings()
        {
            return View();
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