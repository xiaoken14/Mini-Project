using Microsoft.AspNetCore.Mvc;

namespace Mini_Project.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult PatientDashboard()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}
