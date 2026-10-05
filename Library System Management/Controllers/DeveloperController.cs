using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // Public developer documentation page for API endpoints
    public class DeveloperController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
