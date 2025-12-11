using Microsoft.AspNetCore.Mvc;

namespace Mini_Project.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
