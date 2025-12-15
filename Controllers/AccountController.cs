using Microsoft.AspNetCore.Mvc;
using Mini_Project.Data;
using Mini_Project.Models;
using Mini_Project.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Mini_Project.Controllers;

public class AccountController(ApplicationDbContext db, Helper hp) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM vm, string? returnURL)
    {
        var admin = await db.Admins.FirstOrDefaultAsync(a => a.Email == vm.Email);
        if (admin != null)
        {
            if (hp.VerifyPassword(admin.Password, vm.Password))
            {
                await hp.SignIn(admin.Email, "Admin", vm.RememberMe);
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Email == vm.Email);
        if (doctor != null)
        {
            if (hp.VerifyPassword(doctor.Password, vm.Password))
            {
                await hp.SignIn(doctor.Email, "Doctor", vm.RememberMe);
                return RedirectToAction("Index", "Doctor");
            }
        }

        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Email == vm.Email);
        if (patient != null)
        {
            if (!patient.EmailConfirmed)
            {
                ModelState.AddModelError("", "Email not confirmed. Please check your email.");
                return View(vm);
            }

            if (hp.VerifyPassword(patient.Password, vm.Password))
            {
                await hp.SignIn(patient.Email, "Patient", vm.RememberMe);
                return RedirectToAction("Index", "Patient");
            }
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(vm);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (vm.Email != null && (await db.Patients.AnyAsync(p => p.Email == vm.Email) || await db.Doctors.AnyAsync(d => d.Email == vm.Email) || await db.Admins.AnyAsync(a => a.Email == vm.Email)))
        {
            ModelState.AddModelError(nameof(vm.Email), "This email address is already registered.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var patient = new Patient
        {
            Full_Name = vm.Name ?? "",
            Email = vm.Email!,
            Password = hp.HashPassword(vm.Password!),
            Contact_No = vm.PhoneNumber ?? "",
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            EmailConfirmed = false,

        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { email = patient.Email, token = patient.EmailConfirmationToken }, Request.Scheme);
        
        try 
        {
            var mail = new MailMessage();
            mail.To.Add(patient.Email);
            mail.Subject = "Confirm your email";
            mail.Body = $"Please confirm your account by clicking this link: {confirmationLink}";
            hp.SendEmail(mail);
        }
        catch
        {
            // Log error
        }

        return RedirectToAction("RegistrationConfirmation", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Email == email && p.EmailConfirmationToken == token);
        if (patient != null)
        {
            patient.EmailConfirmed = true;
            await db.SaveChangesAsync();
            return RedirectToAction("EmailConfirmed");
        }

        return RedirectToAction("Error");
    }

    [HttpGet]
    public IActionResult EmailConfirmed() => View();

    [HttpGet]
    public IActionResult RegistrationConfirmation() => View();

    [HttpGet]
    public IActionResult DoctorLogin() => View();

    [HttpGet]
    public IActionResult AdminLogin() => View();

    public IActionResult AccessDenied() => View();

    public IActionResult Error() => View();
}