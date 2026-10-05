using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    [Authorize(Roles = "Manager,Admin,Reception")]
    public class NotificationsController : Controller
    {
        private readonly ILibraryRepository _repo;
        public NotificationsController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // GET: /Notifications
        public IActionResult Index()
        {
            var notes = _repo.GetAllNotifications();
            return View(notes);
        }
    }
}
