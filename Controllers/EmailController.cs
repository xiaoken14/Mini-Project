using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace HealthcareApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public EmailController(IEmailService emailService, IWebHostEnvironment environment)
        {
            _emailService = emailService;
            _environment = environment;
        }

        // GET: Email
        public IActionResult Index()
        {
            return View(new EmailViewModel());
        }

        // POST: Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Construct email
                    var mail = new EmailViewModel
                    {
                        Email = vm.Email,
                        Subject = vm.Subject,
                        Body = vm.Body,
                        IsBodyHtml = vm.IsBodyHtml
                    };

                    // File attachment (optional)
                    var attachmentPath = Path.Combine(_environment.ContentRootPath, "Secret.pdf");
                    if (System.IO.File.Exists(attachmentPath))
                    {
                        mail.AttachmentPath = attachmentPath;
                    }

                    // Send email asynchronously
                    bool emailSent = await _emailService.SendEmailAsync(mail);

                    if (emailSent)
                    {
                        TempData["Success"] = "Email sent successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to send email. Please check your SMTP settings.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error sending email: {ex.Message}");
                }
            }

            return View(vm);
        }

        // POST: Email/SendTest
        [HttpPost]
        public async Task<IActionResult> SendTest(string email)
        {
            try
            {
                var testEmail = new EmailViewModel
                {
                    Email = email,
                    Subject = "Test Email from Healthcare App",
                    Body = "<h2>Hello!</h2><p>This is a test email from your Healthcare Application.</p><p>If you received this, your email configuration is working correctly!</p>",
                    IsBodyHtml = true
                };

                bool emailSent = await _emailService.SendEmailAsync(testEmail);

                if (emailSent)
                {
                    return Json(new { success = true, message = "Test email sent successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to send test email." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}