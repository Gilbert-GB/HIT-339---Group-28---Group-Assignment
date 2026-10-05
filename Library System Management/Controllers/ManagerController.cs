using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// ManagerController.cs
// This controller provides management-level statistics and reports for the
// library. Access is restricted to users in the "Manager" role.

namespace Library_System_Management.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ILibraryRepository _repo;
        public ManagerController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // GET: /Manager/Index
        // Index: compute and return basic statistics used by the manager dashboard.
        // The model includes counts of items by status and borrow/fine summaries.
        public IActionResult Index()
        {
            var items = _repo.GetAllItems();
            var records = _repo.GetAllBorrowRecords();

            var model = new
            {
                TotalItems = items.Count(),
                Available = items.Where(i => i.Status == Library_System_Management.Models.ItemStatus.Available).Count(),
                Borrowed = items.Where(i => i.Status == Library_System_Management.Models.ItemStatus.Borrowed).Count(),
                Damaged = items.Where(i => i.Status == Library_System_Management.Models.ItemStatus.Damaged).Count(),
                Destroy = items.Where(i => i.Status == Library_System_Management.Models.ItemStatus.Destroy).Count(),
                TotalBorrows = records.Count(),
                OutstandingBorrows = records.Where(r => r.ReturnedAt == null).Count(),
                TotalFinesCollected = records.Sum(r => r.FinePaid)
            };

            return View(model);
        }
    }
}
