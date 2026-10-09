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

        // POST: /Notifications/RunDueDateCheck
        // Runs the due-date check on demand (it also runs automatically every hour).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RunDueDateCheck()
        {
            var created = _repo.GenerateDueDateNotifications(DateTime.UtcNow);
            TempData["DueCheckMessage"] = created == 0
                ? "Due-date check complete. No new reminders were needed (reminders already sent are not repeated)."
                : $"Due-date check complete. {created} new reminder(s) sent.";
            return RedirectToAction(nameof(Index));
        }
    }
}