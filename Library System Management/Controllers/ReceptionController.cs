using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// ReceptionController: handles front-desk workflows (borrow, return, and borrower management).
// Add clear comments to actions so maintainers and callers understand intended behavior.

namespace Library_System_Management.Controllers
{
    [Authorize(Roles = "Reception")]
    // Only users in the Reception role should access these endpoints.
    public class ReceptionController : Controller
    {
        private readonly ILibraryRepository _repo;

        public ReceptionController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // Index: show borrow records (mapped to a friendly view model in the view).
        // Records are ordered by borrowed date descending.
        public IActionResult Index()
        {
            var records = _repo.GetAllBorrowRecords();
            var vm = records.Select(r => {
                var item = _repo.GetItem(r.ItemId);
                var borrower = _repo.GetBorrower(r.BorrowerId);
                return new Library_System_Management.Models.BorrowRecordViewModel
                {
                    Id = r.Id,
                    BorrowRecordId = r.Id,
                    ItemName = item?.Name ?? r.ItemId.ToString(),
                    ItemCode = item?.LibraryCode ?? string.Empty,
                    BorrowerName = borrower?.FullName ?? borrower?.Email ?? r.BorrowerId.ToString(),
                    BorrowerUsername = borrower?.Username ?? string.Empty,
                    BorrowedAt = r.BorrowedAt,
                    DueAt = r.DueAt,
                    ReturnedAt = r.ReturnedAt,
                    FinePaid = r.FinePaid
                };
            }).OrderByDescending(x => x.BorrowedAt);

            return View(vm);
        }

        // GET: show borrow form to register a new borrow operation.
        [HttpGet]
        public IActionResult Borrow() => View();

        // POST: attempt to borrow an item by library code for the provided borrower details.
        // Returns BadRequest if input is invalid or the item cannot be borrowed.
        [HttpPost]
        public IActionResult Borrow(string code, Borrower borrower, int days = 14)
        {
            if (string.IsNullOrWhiteSpace(code) || borrower == null) return BadRequest();
            var rec = _repo.BorrowItem(code, borrower, days);
            if (rec == null) return BadRequest("Item not available or code invalid.");
            return RedirectToAction("Index");
        }

        // Return: mark a borrow record as returned by id and redirect to index.
        public IActionResult Return(System.Guid id)
        {
            _repo.ReturnItem(id);
            return RedirectToAction("Index");
        }

        // Borrowers: list all registered borrowers.
        public IActionResult Borrowers()
        {
            var b = _repo.GetAllBorrowers();
            return View(b);
        }

        // GET: show create borrower form.
        [HttpGet]
        public IActionResult CreateBorrower() => View();

        // POST: validate and persist a new borrower, then redirect to the borrowers list.
        [HttpPost]
        public IActionResult CreateBorrower(Borrower borrower)
        {
            if (!ModelState.IsValid) return View(borrower);
            _repo.AddBorrower(borrower);
            return RedirectToAction("Borrowers");
        }

        // GET: show edit form for an existing borrower.
        [HttpGet]
        public IActionResult EditBorrower(Guid id)
        {
            var b = _repo.GetBorrower(id);
            if (b == null) return NotFound();
            return View(b);
        }

        // POST: validate and update borrower details, then return to borrowers list.
        [HttpPost]
        public IActionResult EditBorrower(Borrower borrower)
        {
            if (!ModelState.IsValid) return View(borrower);
            _repo.UpdateBorrower(borrower);
            return RedirectToAction("Borrowers");
        }
    }
}
