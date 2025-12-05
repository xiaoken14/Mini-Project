using Microsoft.AspNetCore.Mvc;

namespace Mini_Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Error() => View();
    }
}