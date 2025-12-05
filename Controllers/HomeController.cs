using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Error() => View();
    }
}
