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
                    // Get appointments if PatientId is linked
                    if (user.PatientId.HasValue)
                    {
                        var today = DateTime.Today;
                        
                        // Get upcoming appointments
                        upcomingAppointments = await _context.Appointments
                            .Include(a => a.Doctor)
                            .Include(a => a.Patient)
                            .Where(a => a.PatientId == user.PatientId.Value && 
                                       a.AppointmentDate >= today &&
                                       a.Status != "Cancelled")
                            .OrderBy(a => a.AppointmentDate)
                            .ThenBy(a => a.AppointmentTime)
                            .Take(5)
                            .ToListAsync();

                        // Get recent appointments (past)
                        recentAppointments = await _context.Appointments
                            .Include(a => a.Doctor)
                            .Include(a => a.Patient)
                            .Where(a => a.PatientId == user.PatientId.Value && 
                                       a.AppointmentDate < today)
                            .OrderByDescending(a => a.AppointmentDate)
                            .ThenByDescending(a => a.AppointmentTime)
                            .Take(5)
                            .ToListAsync();

                        // Get total appointments count
                        totalAppointments = await _context.Appointments
                            .CountAsync(a => a.PatientId == user.PatientId.Value);

                        // Get pending appointments count
                        pendingAppointments = await _context.Appointments
                            .CountAsync(a => a.PatientId == user.PatientId.Value && 
                                           a.Status == "Booked" &&
                                           a.AppointmentDate >= today);
                    }
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

                var appointments = new List<Appointment>();

                // Get appointments if PatientId is linked
                if (user.PatientId.HasValue)
                {
                    appointments = await _context.Appointments
                        .Include(a => a.Doctor)
                        .Include(a => a.Patient)
                        .Where(a => a.PatientId == user.PatientId.Value)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.AppointmentTime)
                        .ToListAsync();
                }

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

            if (!user.PatientId.HasValue)
            {
                TempData["ErrorMessage"] = "Patient profile not properly configured.";
                return RedirectToAction(nameof(Appointments));
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.PatientId == user.PatientId.Value);

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

        public async Task<IActionResult> Payments()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new PatientPaymentsViewModel();

                // Get data if PatientId is linked
                if (user.PatientId.HasValue)
                {
                    // Get unpaid appointments
                    var unpaidAppointments = await _context.Appointments
                        .Include(a => a.Doctor)
                        .Where(a => a.PatientId == user.PatientId.Value && 
                                   a.Status == "Booked" &&
                                   a.AppointmentDate >= DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .ToListAsync();

                    // Get payment history
                    var paymentHistory = await _context.Payments
                        .Include(p => p.Appointment)
                        .ThenInclude(a => a.Doctor)
                        .Where(p => p.Appointment.PatientId == user.PatientId.Value)
                        .OrderByDescending(p => p.PaymentDate)
                        .ToListAsync();

                    // Calculate totals
                    var totalUnpaid = unpaidAppointments.Sum(a => (a.ConsultationFee ?? 300m) * 0.8m); // With 20% discount
                    var totalPaid = paymentHistory.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount);

                    viewModel.UnpaidAppointments = unpaidAppointments;
                    viewModel.PaymentHistory = paymentHistory;
                    viewModel.TotalUnpaid = totalUnpaid;
                    viewModel.TotalPaid = totalPaid;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payments: {ex.Message}");
                return View(new PatientPaymentsViewModel());
            }
        }

        public async Task<IActionResult> MakePayment(int appointmentId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!user.PatientId.HasValue)
                {
                    TempData["ErrorMessage"] = "Patient profile not properly configured.";
                    return RedirectToAction("Payments");
                }

                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && 
                                           a.PatientId == user.PatientId.Value);

                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }

                // Check if payment already exists
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

                if (existingPayment != null && existingPayment.PaymentStatus == "Completed")
                {
                    TempData["ErrorMessage"] = "Payment has already been completed for this appointment.";
                    return RedirectToAction("Appointments");
                }

                // Calculate payment amount (base consultation fee)
                decimal baseAmount = appointment.ConsultationFee ?? 300.00m; // Default consultation fee
                
                // Apply discount based on patient category or age
                decimal discountPercentage = 0;
                if (user.DateOfBirth.HasValue)
                {
                    var age = DateTime.Today.Year - user.DateOfBirth.Value.Year;
                    if (age >= 60) // Senior citizen discount
                    {
                        discountPercentage = 20;
                    }
                }

                // You could also check for student status here
                // if (patient.Category == "Student") discountPercentage = 20;

                var viewModel = new PaymentViewModel
                {
                    AppointmentId = appointmentId,
                    OriginalAmount = baseAmount,
                    DiscountPercentage = discountPercentage,
                    Amount = baseAmount - (baseAmount * discountPercentage / 100),
                    Appointment = appointment,
                    CardholderName = $"{user.FirstName} {user.LastName}".Trim()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MakePayment GET: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the payment page.";
                return RedirectToAction("Appointments");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(PaymentViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!user.PatientId.HasValue)
                {
                    TempData["ErrorMessage"] = "Patient profile not properly configured.";
                    return RedirectToAction("Payments");
                }

                // Reload appointment for security
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && 
                                           a.PatientId == user.PatientId.Value);

                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }

                // Remove validation for navigation properties
                ModelState.Remove("Appointment");

                if (ModelState.IsValid)
                {
                    // Check if payment already exists
                    var existingPayment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.AppointmentId == model.AppointmentId);

                    if (existingPayment != null && existingPayment.PaymentStatus == "Completed")
                    {
                        TempData["ErrorMessage"] = "Payment has already been completed for this appointment.";
                        return RedirectToAction("Appointments");
                    }

                    // Simulate payment processing (in real app, integrate with payment gateway)
                    var isPaymentSuccessful = await ProcessPayment(model);

                    if (isPaymentSuccessful)
                    {
                        // Create or update payment record
                        if (existingPayment != null)
                        {
                            existingPayment.Amount = model.FinalAmount;
                            existingPayment.PaymentStatus = "Completed";
                            existingPayment.PaymentMethod = "Credit Card";
                            existingPayment.PaymentDate = DateTime.UtcNow;
                            _context.Update(existingPayment);
                        }
                        else
                        {
                            var payment = new Payment
                            {
                                AppointmentId = model.AppointmentId,
                                Amount = model.FinalAmount,
                                PaymentStatus = "Completed",
                                PaymentMethod = "Credit Card",
                                PaymentDate = DateTime.UtcNow
                            };
                            _context.Payments.Add(payment);
                        }

                        // Update appointment status
                        appointment.Status = "Confirmed";
                        appointment.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Payment of RM{model.FinalAmount:F2} completed successfully! Your appointment is now confirmed.";
                        return RedirectToAction("Appointments");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Payment processing failed. Please check your card details and try again.");
                    }
                }

                // If we got this far, something failed, redisplay form
                model.Appointment = appointment;
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MakePayment POST: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while processing your payment.";
                return RedirectToAction("Appointments");
            }
        }

        private async Task<bool> ProcessPayment(PaymentViewModel model)
        {
            // Simulate payment processing
            // In a real application, you would integrate with a payment gateway like Stripe, PayPal, etc.
            
            await Task.Delay(1000); // Simulate processing time
            
            // Basic validation (in real app, this would be done by payment gateway)
            if (string.IsNullOrEmpty(model.CardNumber) || 
                string.IsNullOrEmpty(model.CardholderName) || 
                string.IsNullOrEmpty(model.CVV))
            {
                return false;
            }

            // Simulate 95% success rate (for demo purposes)
            var random = new Random();
            return random.Next(1, 101) <= 95;
        }
    }
}