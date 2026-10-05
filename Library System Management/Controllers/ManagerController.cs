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

        // GET: /Manager/ExportCsv
        // Exports a simple CSV report of items and borrow counts
        public IActionResult ExportCsv()
        {
            var items = _repo.GetAllItems();
            var records = _repo.GetAllBorrowRecords();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("LibraryCode,Name,Type,Branch,Status,BorrowCount");
            foreach(var it in items)
            {
                var type = it.GetType().Name;
                var branchName = string.Empty;
                if (it.BranchId != null)
                {
                    var b = _repo.GetBranch(it.BranchId.Value);
                    branchName = b?.Name ?? string.Empty;
                }
                var borrowCount = records.Count(r => r.ItemId == it.Id);
                csv.AppendLine($"{it.LibraryCode},{Escape(it.Name)},{type},{Escape(branchName)},{it.Status},{borrowCount}");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "items-report.csv");
        }

        private static string Escape(string s) => s?.Replace("\"", "\"\"") ?? string.Empty;
    }
}
