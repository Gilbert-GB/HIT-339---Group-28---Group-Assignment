using System.Globalization;
using System.Text;
using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                Available = items.Where(i => i.Status == ItemStatus.Available).Count(),
                Borrowed = items.Where(i => i.Status == ItemStatus.Borrowed).Count(),
                Damaged = items.Where(i => i.Status == ItemStatus.Damaged).Count(),
                Destroy = items.Where(i => i.Status == ItemStatus.Destroy).Count(),
                TotalBorrows = records.Count(),
                OutstandingBorrows = records.Where(r => r.ReturnedAt == null).Count(),
                TotalFinesCollected = records.Where(r => r.FineSettled).Sum(r => r.FinePaid),
                OutstandingFines = records.Where(r => r.FinePaid > 0 && !r.FineSettled).Sum(r => r.FinePaid)
            };

            return View(model);
        }

        // GET: /Manager/ExportCsv
        // Exports a simple CSV report of items and borrow counts
        public IActionResult ExportCsv()
        {
            var items = _repo.GetAllItems();
            var branches = _repo.GetAllBranches().ToDictionary(b => b.Id);
            var borrowCounts = BorrowCounts();

            var csv = new StringBuilder();
            csv.AppendLine(Csv("LibraryCode", "Name", "Type", "Branch", "Status", "BorrowCount"));
            foreach (var it in items)
            {
                csv.AppendLine(Csv(it.LibraryCode, it.Name, it.GetType().Name, BranchName(it, branches),
                    it.Status, borrowCounts.GetValueOrDefault(it.Id)));
            }
            return CsvFile(csv, "items-report");
        }

        // GET: /Manager/ExportBorrowingCsv
        // Borrowing statistics: a summary block followed by every borrow record,
        // with its item, borrower, branch, dates, current status, fine and fine status.
        public IActionResult ExportBorrowingCsv()
        {
            var records = _repo.GetAllBorrowRecords().OrderByDescending(r => r.BorrowedAt).ToList();
            var items = _repo.GetAllItems().ToDictionary(i => i.Id);
            var borrowers = _repo.GetAllBorrowers().ToDictionary(b => b.Id);
            var branches = _repo.GetAllBranches().ToDictionary(b => b.Id);
            var now = DateTime.UtcNow;

            var csv = new StringBuilder();
            csv.AppendLine("Borrowing Statistics Summary");
            csv.AppendLine(Csv("Total Borrows", records.Count));
            csv.AppendLine(Csv("Active", records.Count(r => RecordStatus(r, now) == "Active")));
            csv.AppendLine(Csv("Overdue", records.Count(r => RecordStatus(r, now) == "Overdue")));
            csv.AppendLine(Csv("Returned", records.Count(r => r.ReturnedAt != null)));
            csv.AppendLine();

            csv.AppendLine("Borrow Records");
            csv.AppendLine(Csv("Library Code", "Item", "Type", "Branch", "Borrower", "Borrower Email",
                "Borrowed", "Due", "Returned", "Status", "Fine", "Fine Status"));
            foreach (var r in records)
            {
                items.TryGetValue(r.ItemId, out var item);
                borrowers.TryGetValue(r.BorrowerId, out var borrower);
                csv.AppendLine(Csv(item?.LibraryCode, item?.Name, item?.GetType().Name, BranchName(item, branches),
                    borrower?.FullName, borrower?.Email,
                    r.BorrowedAt, r.DueAt, r.ReturnedAt, RecordStatus(r, now), r.FinePaid, FineStatus(r)));
            }
            return CsvFile(csv, "borrowing-statistics");
        }

        // GET: /Manager/ExportFineAuditCsv
        // Fine revenue audit: every borrow record that incurred a fine with its payment status,
        // followed by assessed, collected and outstanding totals per branch and overall.
        public IActionResult ExportFineAuditCsv()
        {
            var fined = _repo.GetAllBorrowRecords()
                .Where(r => r.FinePaid > 0)
                .OrderBy(r => r.ReturnedAt)
                .ToList();
            var items = _repo.GetAllItems().ToDictionary(i => i.Id);
            var borrowers = _repo.GetAllBorrowers().ToDictionary(b => b.Id);
            var branches = _repo.GetAllBranches().ToDictionary(b => b.Id);

            var csv = new StringBuilder();
            csv.AppendLine("Fine Revenue Audit");
            csv.AppendLine(Csv("Returned", "Due", "Days Late", "Library Code", "Item", "Branch",
                "Borrower", "Borrower Email", "Fine Amount", "Payment Status", "Paid On", "Payment Method", "Receipt"));
            foreach (var r in fined)
            {
                items.TryGetValue(r.ItemId, out var item);
                borrowers.TryGetValue(r.BorrowerId, out var borrower);
                var daysLate = r.ReturnedAt.HasValue
                    ? Math.Max(0, (r.ReturnedAt.Value.Date - r.DueAt.Date).Days)
                    : 0;
                csv.AppendLine(Csv(r.ReturnedAt, r.DueAt, daysLate, item?.LibraryCode, item?.Name,
                    BranchName(item, branches), borrower?.FullName, borrower?.Email, r.FinePaid,
                    FineStatus(r), r.FineSettledAt, r.PaymentMethod, r.PaymentReference));
            }
            csv.AppendLine();

            csv.AppendLine("Fine Revenue by Branch");
            csv.AppendLine(Csv("Branch", "Fined Loans", "Assessed", "Collected", "Outstanding"));
            var byBranch = fined
                .GroupBy(r => BranchName(items.GetValueOrDefault(r.ItemId), branches))
                .OrderBy(g => g.Key);
            foreach (var g in byBranch)
            {
                csv.AppendLine(Csv(g.Key, g.Count(), g.Sum(r => r.FinePaid),
                    g.Where(r => r.FineSettled).Sum(r => r.FinePaid),
                    g.Where(r => !r.FineSettled).Sum(r => r.FinePaid)));
            }
            csv.AppendLine();

            var assessed = fined.Sum(r => r.FinePaid);
            csv.AppendLine(Csv("Total Assessed", assessed));
            csv.AppendLine(Csv("Total Collected", fined.Where(r => r.FineSettled).Sum(r => r.FinePaid)));
            csv.AppendLine(Csv("Total Outstanding", fined.Where(r => !r.FineSettled).Sum(r => r.FinePaid)));
            csv.AppendLine(Csv("Fined Loans", fined.Count));
            csv.AppendLine(Csv("Average Fine", fined.Count == 0 ? 0m : assessed / fined.Count));

            return CsvFile(csv, "fine-revenue-audit");
        }

        // GET: /Manager/ExportInventoryHealthCsv
        // Inventory health: status counts and availability per branch, then lists of
        // items needing attention (damaged or destroyed) and items never borrowed.
        public IActionResult ExportInventoryHealthCsv()
        {
            var items = _repo.GetAllItems().ToList();
            var branches = _repo.GetAllBranches().ToDictionary(b => b.Id);
            var borrowCounts = BorrowCounts();

            var csv = new StringBuilder();
            csv.AppendLine("Inventory Health by Branch");
            csv.AppendLine(Csv("Branch", "Total Items", "Available", "Borrowed", "Damaged", "Destroy",
                "% Available", "Never Borrowed"));

            var groups = items.GroupBy(i => BranchName(i, branches)).OrderBy(g => g.Key).ToList();
            foreach (var g in groups)
            {
                csv.AppendLine(HealthRow(g.Key, g.ToList(), borrowCounts));
            }
            csv.AppendLine(HealthRow("All Branches", items, borrowCounts));
            csv.AppendLine();

            csv.AppendLine("Items Needing Attention");
            csv.AppendLine(Csv("Library Code", "Name", "Type", "Branch", "Status", "Times Borrowed"));
            foreach (var it in items.Where(i => i.Status == ItemStatus.Damaged || i.Status == ItemStatus.Destroy)
                                    .OrderBy(i => i.LibraryCode))
            {
                csv.AppendLine(Csv(it.LibraryCode, it.Name, it.GetType().Name, BranchName(it, branches),
                    it.Status, borrowCounts.GetValueOrDefault(it.Id)));
            }
            csv.AppendLine();

            csv.AppendLine("Never Borrowed Items");
            csv.AppendLine(Csv("Library Code", "Name", "Type", "Branch", "Status"));
            foreach (var it in items.Where(i => !borrowCounts.ContainsKey(i.Id)).OrderBy(i => i.LibraryCode))
            {
                csv.AppendLine(Csv(it.LibraryCode, it.Name, it.GetType().Name, BranchName(it, branches), it.Status));
            }

            return CsvFile(csv, "inventory-health");
        }

        // ---- Helpers ----

        private Dictionary<Guid, int> BorrowCounts() =>
            _repo.GetAllBorrowRecords().GroupBy(r => r.ItemId).ToDictionary(g => g.Key, g => g.Count());

        private static string HealthRow(string branch, List<Item> group, Dictionary<Guid, int> borrowCounts)
        {
            var total = group.Count;
            var available = group.Count(i => i.Status == ItemStatus.Available);
            var percentAvailable = total == 0 ? 0.0 : available * 100.0 / total;
            return Csv(branch, total, available,
                group.Count(i => i.Status == ItemStatus.Borrowed),
                group.Count(i => i.Status == ItemStatus.Damaged),
                group.Count(i => i.Status == ItemStatus.Destroy),
                percentAvailable,
                group.Count(i => !borrowCounts.ContainsKey(i.Id)));
        }

        private static string RecordStatus(BorrowRecord r, DateTime now) =>
            r.ReturnedAt != null ? "Returned" : r.DueAt < now ? "Overdue" : "Active";

        private static string FineStatus(BorrowRecord r) =>
            r.FinePaid <= 0 ? string.Empty : r.FineSettled ? "Paid" : "Outstanding";

        private static string BranchName(Item? item, Dictionary<Guid, Branch> branches) =>
            item?.BranchId is Guid id && branches.TryGetValue(id, out var b) ? b.Name : "Unassigned";

        // Builds one CSV line, quoting any value that contains a comma, quote or line break.
        private static string Csv(params object?[] values) => string.Join(",", values.Select(Field));

        private static string Field(object? value)
        {
            var s = value switch
            {
                null => string.Empty,
                DateTime d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                decimal m => m.ToString("0.00", CultureInfo.InvariantCulture),
                double x => x.ToString("0.0", CultureInfo.InvariantCulture),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            };
            return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
                ? "\"" + s.Replace("\"", "\"\"") + "\""
                : s;
        }

        // Returns the CSV as a download. The UTF-8 byte order mark helps Excel read it correctly.
        private FileContentResult CsvFile(StringBuilder csv, string name)
        {
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"{name}-{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}