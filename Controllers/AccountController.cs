using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthcareApp.Models;
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

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        // Get user roles
                        var roles = await _userManager.GetRolesAsync(user);

                        // Redirect based on role
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Doctor"))
                        {
                            return RedirectToAction("Index", "Doctor");
                        }
                        else if (roles.Contains("Patient"))
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
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            // Admin login successful
                            return LocalRedirect(returnUrl ?? Url.Action("Index", "Admin") ?? "/Admin");
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
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            // Doctor login successful
                            return LocalRedirect(returnUrl ?? Url.Action("Index", "Doctor") ?? "/Doctor");
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
                        var result = await _signInManager.PasswordSignInAsync(user.UserName,
                            model.Password,
                            model.RememberMe,
                            lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            // Patient login successful
                            return LocalRedirect(returnUrl ?? Url.Action("Dashboard", "Patient") ?? "/Patient");
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

                    // Automatically sign in
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to different pages based on role
                    return roleName switch
                    {
                        "Admin" => RedirectToAction("Index", "Admin"),
                        "Doctor" => RedirectToAction("Index", "Doctor"),
                        "Patient" => RedirectToAction("Dashboard", "Patient"),
                        _ => RedirectToAction("Index", "Home")
                    };
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
    }
}