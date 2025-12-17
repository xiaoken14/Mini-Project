using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthcareApp.Models;
using HealthcareApp.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IOTPService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _otpService = otpService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.UserName != null)
                {
                    // Check password manually
                    var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (passwordValid)
                    {
                        // Check if email is confirmed
                        if (!user.EmailConfirmed)
                        {
                            // Get user roles to determine if they need email verification
                            var roles = await _userManager.GetRolesAsync(user);
                            
                            if (roles.Contains("Admin") || roles.Contains("Doctor"))
                            {
                                // For existing admin/doctor users, auto-confirm email
                                user.EmailConfirmed = true;
                                await _userManager.UpdateAsync(user);
                            }
                            else
                            {
                                // For patients, redirect to email verification
                                return RedirectToAction("VerifyOTP", new { email = user.Email });
                            }
                        }
                        
                        // Sign in the user
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        
                        // Get user roles for redirect
                        var userRoles = await _userManager.GetRolesAsync(user);

                        // Redirect based on role
                        if (userRoles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (userRoles.Contains("Doctor"))
                        {
                            return RedirectToAction("Index", "Doctor");
                        }
                        else if (userRoles.Contains("Patient"))
                        {
                            return RedirectToAction("Dashboard", "Patient");
                        }
                        else
                        {
                            // Default redirect to home page
                            return LocalRedirect(returnUrl ?? Url.Content("~/"));
                        }
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // GET: /Account/LoginAdmin
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginAdmin(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.IsAdminLogin = true;
            return View("LoginAdmin");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.UserName != null)
                {
                    // Check if user is an admin
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (isAdmin)
                    {
                        // Check password manually for admin users
                        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                        if (passwordValid)
                        {
                            // For admin users, bypass email confirmation requirement
                            if (!user.EmailConfirmed)
                            {
                                user.EmailConfirmed = true;
                                await _userManager.UpdateAsync(user);
                            }
                            
                            // Sign in the admin user
                            await _signInManager.SignInAsync(user, model.RememberMe);
                            return LocalRedirect(returnUrl ?? Url.Action("Index", "Admin") ?? "/Admin");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            return View("LoginAdmin", model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "You are not authorized to access the admin panel.");
                        return View("LoginAdmin", model);
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View("LoginAdmin", model);
        }

        // GET: /Account/LoginDoctor
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginDoctor(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.IsDoctorLogin = true;
            return View("LoginDoctor");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginDoctor(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.UserName != null)
                {
                    // Check if user is a doctor
                    var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
                    if (isDoctor)
                    {
                        // Check password manually for doctor users
                        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                        if (passwordValid)
                        {
                            // For existing doctor users, bypass email confirmation requirement
                            if (!user.EmailConfirmed)
                            {
                                user.EmailConfirmed = true;
                                await _userManager.UpdateAsync(user);
                            }
                            
                            // Sign in the doctor user
                            await _signInManager.SignInAsync(user, model.RememberMe);
                            return LocalRedirect(returnUrl ?? Url.Action("Index", "Doctor") ?? "/Doctor");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            return View("LoginDoctor", model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "You are not registered as a doctor.");
                        return View("LoginDoctor", model);
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View("LoginDoctor", model);
        }

        // GET: /Account/LoginPatient
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginPatient(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("LoginPatient");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPatient(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.UserName != null)
                {
                    // Check if user is a patient
                    var isPatient = await _userManager.IsInRoleAsync(user, "Patient");
                    if (isPatient)
                    {
                        // Check password manually for patient users
                        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                        if (passwordValid)
                        {
                            // Check if email is confirmed
                            if (!user.EmailConfirmed)
                            {
                                // For new patients, redirect to email verification
                                return RedirectToAction("VerifyOTP", new { email = user.Email });
                            }
                            
                            // Sign in the patient user
                            await _signInManager.SignInAsync(user, model.RememberMe);
                            return LocalRedirect(returnUrl ?? Url.Action("Dashboard", "Patient") ?? "/Patient");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            return View("LoginPatient", model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "You are not registered as a patient.");
                        return View("LoginPatient", model);
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View("LoginPatient", model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Create role list for view
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                // Set role-specific properties
                switch (model.Role)
                {
                    case UserRole.Doctor:
                        user.Specialization = model.Specialization;
                        user.LicenseNumber = model.LicenseNumber;
                        user.PhoneNumber = model.EmergencyContact;
                        break;
                    case UserRole.Patient:
                        user.DateOfBirth = model.DateOfBirth;
                        user.Address = model.Address;
                        user.EmergencyContact = model.EmergencyContact;
                        user.PhoneNumber = model.EmergencyContact;
                        break;
                    case UserRole.Admin:
                        // Admin may not need these properties
                        break;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign role based on selection
                    string roleName = model.Role.ToString();
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Generate and send OTP for email verification
                    var otp = _otpService.GenerateOTP();
                    var otpExpiry = _otpService.GetOTPExpiry();
                    
                    // Store OTP in user record
                    user.EmailOTP = otp;
                    user.OTPExpiry = otpExpiry;
                    user.OTPAttempts = 0;
                    await _userManager.UpdateAsync(user);

                    // Send OTP email
                    var emailSubject = "🏥 Healthcare App - Email Verification";
                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; text-align: center;'>
                                <h1 style='color: white; margin: 0;'>🏥 Healthcare App</h1>
                            </div>
                            <div style='padding: 30px; background-color: #f8f9fa;'>
                                <h2 style='color: #333; text-align: center;'>Email Verification Required</h2>
                                <p>Hello {user.FirstName} {user.LastName},</p>
                                <p>Thank you for registering with Healthcare App! To complete your registration, please verify your email address.</p>
                                
                                <div style='background-color: white; padding: 20px; border-radius: 10px; text-align: center; margin: 20px 0;'>
                                    <h3 style='color: #667eea; margin-bottom: 10px;'>Your Verification Code:</h3>
                                    <div style='font-size: 32px; font-weight: bold; color: #764ba2; letter-spacing: 5px; font-family: monospace;'>
                                        {otp}
                                    </div>
                                    <p style='color: #666; margin-top: 10px; font-size: 14px;'>
                                        This code will expire in 10 minutes
                                    </p>
                                </div>
                                
                                <p>Please enter this code on the verification page to activate your account.</p>
                                <p style='color: #666; font-size: 14px;'>
                                    If you didn't create an account with us, please ignore this email.
                                </p>
                            </div>
                            <div style='background-color: #333; color: white; padding: 15px; text-align: center; font-size: 12px;'>
                                © 2024 Healthcare App. All rights reserved.
                            </div>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email!, emailSubject, emailBody, true);

                    // Redirect to OTP verification page
                    return RedirectToAction("VerifyOTP", new { email = user.Email });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Reload role list
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/RedirectToRoleLogin
        [HttpGet]
        public IActionResult RedirectToRoleLogin(string role)
        {
            return role?.ToLower() switch
            {
                "admin" => RedirectToAction("LoginAdmin"),
                "doctor" => RedirectToAction("LoginDoctor"),
                "patient" => RedirectToAction("LoginPatient"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/VerifyOTP
        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var model = new VerifyOTPViewModel
            {
                Email = email
            };

            return View(model);
        }

        // POST: /Account/VerifyOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                // Check if user is already confirmed
                if (user.EmailConfirmed)
                {
                    TempData["Message"] = "Your email is already verified. You can now log in.";
                    return RedirectToAction("Login");
                }

                // Validate OTP
                if (_otpService.ValidateOTP(model.OTP, user.EmailOTP, user.OTPExpiry))
                {
                    // Mark email as confirmed
                    user.EmailConfirmed = true;
                    user.EmailOTP = null;
                    user.OTPExpiry = null;
                    user.OTPAttempts = 0;
                    
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Sign in the user
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        // Get user role and redirect accordingly
                        var roles = await _userManager.GetRolesAsync(user);
                        var roleName = roles.FirstOrDefault();

                        TempData["SuccessMessage"] = "Email verified successfully! Welcome to Healthcare App.";

                        return roleName switch
                        {
                            "Admin" => RedirectToAction("Index", "Admin"),
                            "Doctor" => RedirectToAction("Index", "Doctor"),
                            "Patient" => RedirectToAction("Dashboard", "Patient"),
                            _ => RedirectToAction("Index", "Home")
                        };
                    }
                }
                else
                {
                    // Increment failed attempts
                    user.OTPAttempts++;
                    await _userManager.UpdateAsync(user);

                    if (user.OTPAttempts >= 5)
                    {
                        ModelState.AddModelError(string.Empty, "Too many failed attempts. Please request a new verification code.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid or expired verification code. Please try again.");
                    }
                }
            }

            return View(model);
        }

        // POST: /Account/ResendOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Register");
            }

            if (user.EmailConfirmed)
            {
                TempData["Message"] = "Your email is already verified.";
                return RedirectToAction("Login");
            }

            // Generate new OTP
            var otp = _otpService.GenerateOTP();
            var otpExpiry = _otpService.GetOTPExpiry();
            
            user.EmailOTP = otp;
            user.OTPExpiry = otpExpiry;
            user.OTPAttempts = 0;
            await _userManager.UpdateAsync(user);

            // Send new OTP email
            var emailSubject = "🏥 Healthcare App - New Verification Code";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>🏥 Healthcare App</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f8f9fa;'>
                        <h2 style='color: #333; text-align: center;'>New Verification Code</h2>
                        <p>Hello {user.FirstName} {user.LastName},</p>
                        <p>You requested a new verification code. Here's your new code:</p>
                        
                        <div style='background-color: white; padding: 20px; border-radius: 10px; text-align: center; margin: 20px 0;'>
                            <h3 style='color: #667eea; margin-bottom: 10px;'>Your New Verification Code:</h3>
                            <div style='font-size: 32px; font-weight: bold; color: #764ba2; letter-spacing: 5px; font-family: monospace;'>
                                {otp}
                            </div>
                            <p style='color: #666; margin-top: 10px; font-size: 14px;'>
                                This code will expire in 10 minutes
                            </p>
                        </div>
                        
                        <p>Please enter this code on the verification page to activate your account.</p>
                    </div>
                    <div style='background-color: #333; color: white; padding: 15px; text-align: center; font-size: 12px;'>
                        © 2024 Healthcare App. All rights reserved.
                    </div>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, emailSubject, emailBody, true);

            TempData["Message"] = "A new verification code has been sent to your email.";
            return RedirectToAction("VerifyOTP", new { email = user.Email });
        }

        // GET: /Account/GetOTP (Development helper)
        [HttpGet]
        public async Task<IActionResult> GetOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email is required" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (string.IsNullOrEmpty(user.EmailOTP))
            {
                return Json(new { success = false, message = "No OTP found for this user" });
            }

            // In development, also check the development email service
            var devOtp = DevelopmentEmailService.GetOTPForEmail(email);
            
            return Json(new { 
                success = true, 
                otp = user.EmailOTP,
                devOtp = devOtp,
                message = $"OTP expires at: {user.OTPExpiry?.ToString("HH:mm:ss")}"
            });
        }

        // GET: /Account/ShowRecentOTPs (Development helper)
        [HttpGet]
        public IActionResult ShowRecentOTPs()
        {
            try
            {
                var filePath = "otp_codes.txt";
                if (System.IO.File.Exists(filePath))
                {
                    var lines = System.IO.File.ReadAllLines(filePath).TakeLast(10).ToArray();
                    return Json(new { success = true, otps = lines });
                }
                else
                {
                    return Json(new { success = false, message = "No OTP file found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error reading OTP file: {ex.Message}" });
            }
        }
    }
}