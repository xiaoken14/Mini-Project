using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Schedule()
        {
            return View();
        }
        
        public IActionResult Patients()
        {
            return View();
        }
        
        public IActionResult Appointments()
        {
            return View();
        }
    }
}