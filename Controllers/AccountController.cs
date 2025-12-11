using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mini_Project.ViewModels;
using System.Threading.Tasks;
using Mini_Project.Services;
using Mini_Project.Data;
using Mini_Project.Models;
using System;
using System.Linq;

namespace Mini_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Patient", "Home");
                }
                if (result.RequiresTwoFactor)
                {
                    // Handle two-factor authentication
                    return RedirectToAction("LoginWith2fa", new { RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                    return View(model);
                }
                if (result.IsNotAllowed)
                {
                    // Check if email is not confirmed
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        // Resend verification email
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        await _emailSender.SendEmailAsync(model.Email, "Verify Your Email", 
                            $"Your verification code is: {code}");
                        
                        TempData["VerificationSent"] = true;
                        return RedirectToAction("VerifyEmail", new { email = model.Email });
                    }
                    
                    ModelState.AddModelError(string.Empty, "You must confirm your email before you can log in.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                // Create identity user
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure "Patient" role exists
                    if (!await _roleManager.RoleExistsAsync("Patient"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Patient"));
                    }

                    // Add role
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Create patient profile
                    var patient = new Patient
                    {
                        UserId = user.Id,
                        Full_Name = model.Name,
                        Email = model.Email,
                        Contact_No = model.PhoneNumber,
                        CreatedDate = DateTime.Now
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    // Generate verification code
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Send verification email
                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Verify Your Email",
                        $"Your verification code is: {code}"
                    );

                    // Store email in TempData for verification page
                    TempData["RegisteredEmail"] = model.Email;
                    TempData["RegistrationSuccess"] = true;

                    // Redirect to verification page
                    return RedirectToAction("VerifyEmail", new { email = model.Email });
                }

                // Show identity errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            // Use email from parameter or TempData
            var emailToVerify = email ?? TempData["RegisteredEmail"] as string;
            
            if (string.IsNullOrEmpty(emailToVerify))
            {
                return RedirectToAction("Register");
            }

            var model = new EmailVerificationVM 
            { 
                Email = emailToVerify 
            };
            
            // Pass success message
            if (TempData["RegistrationSuccess"] != null)
            {
                ViewBag.SuccessMessage = "Registration successful! Please check your email for verification code.";
            }
            if (TempData["VerificationSent"] != null)
            {
                ViewBag.SuccessMessage = "New verification code has been sent to your email.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(EmailVerificationVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Email already confirmed
                    return RedirectToAction("EmailConfirmed");
                }

                var result = await _userManager.ConfirmEmailAsync(user, model.VerificationCode);
                if (result.Succeeded)
                {
                    // Auto login after successful verification
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("EmailConfirmed");
                }
                
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    ModelState.AddModelError(string.Empty, "Invalid or expired verification code. Please request a new one.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid verification code.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Email already confirmed
                    return RedirectToAction("Login");
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailSender.SendEmailAsync(
                    email, 
                    "Verify Your Email", 
                    $"Your verification code is: {code}"
                );

                TempData["VerificationSent"] = true;
                TempData["Message"] = "New verification code has been sent to your email.";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }

            return RedirectToAction("VerifyEmail", new { email = email });
        }

        [HttpGet]
        public IActionResult EmailConfirmed()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DoctorLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
