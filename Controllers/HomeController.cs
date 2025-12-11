using Microsoft.AspNetCore.Mvc;
using Mini_Project.Models;

namespace Mini_Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Add your login logic here
            if (username == "admin" && password == "password")
            {
                // Successful login
                return RedirectToAction("Index");
            }
            else
            {
                // Failed login
                ViewData["Error"] = "Invalid username or password";
                return View("Index");
            }
        }

        public IActionResult Patient()
        {
            // Mock patient data for demonstration
            var patient = new Patient
            {
                Patient_ID = 1,
                Full_Name = "John Doe",
                Email = "john.doe@example.com",
                Contact_No = "123-456-7890"
            };
            return View(patient);
        }

        public IActionResult Error() => View();
    }
}
