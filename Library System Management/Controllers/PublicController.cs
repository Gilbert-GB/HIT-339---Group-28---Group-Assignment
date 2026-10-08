using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // PublicController: handles public (anonymous) search and browsing of catalog items.
    // Supports query, type, status and branch filters.
    public class PublicController : Controller
    {
        private readonly ILibraryRepository _repo;

        public PublicController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // Details: show full details for a single item
        public IActionResult Details(System.Guid id)
        {
            var it = _repo.GetItem(id);
            if (it == null) return NotFound();
            var reservations = _repo.GetReservationsForItem(id);
            ViewData["ReservationCount"] = reservations.Count();
            return View(it);
        }

        // POST: Place a reservation on an item (public-facing). Accepts item id and an email to identify the borrower.
        [HttpPost]
        public IActionResult Reserve(Guid id, string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email required");
            var item = _repo.GetItem(id);
            if (item == null) return NotFound();

            // find or create borrower by email
            var borrower = _repo.GetAllBorrowers().FirstOrDefault(b => string.Equals(b.Email, email, StringComparison.OrdinalIgnoreCase));
            if (borrower == null)
            {
                borrower = new Borrower { Email = email, FullName = email };
                _repo.AddBorrower(borrower);
            }

            // add reservation
            var res = new Reservation { ItemId = id, BorrowerId = borrower.Id };
            _repo.AddReservation(res);

            TempData["ReservationMessage"] = "Your reservation has been placed. We will notify you when the item becomes available.";
            return RedirectToAction("Details", new { id });
        }

        // Index: public search page. Optional parameters:
        // - q: query against name, description, library code, author or artist
        // - type: item type filter (book, music, toy, all)
        // - status: item status filter (Available, Borrowed, etc.)
        // - branch: branch id filter (all branches if empty)
        public IActionResult Index(string? q, string? type, string? status, string? branch)
        {
            var items = _repo.GetAllItems();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                items = items.Where(i =>
                    (i.Name ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Description ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.LibraryCode ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i is Book b && (b.Author ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase))
                    || (i is Music m && (m.Artist ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            // filter by type if provided
            if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
            {
                var t = type.ToLowerInvariant();
                items = items.Where(i =>
                    (t == "book" && i is Book) ||
                    (t == "music" && i is Music) ||
                    (t == "toy" && i is Toy)
                );
            }

            // filter by item status if provided
            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<ItemStatus>(status, true, out var st))
                {
                    items = items.Where(i => i.Status == st);
                }
            }

            // filter by branch if provided
            if (Guid.TryParse(branch, out var branchId))
            {
                items = items.Where(i => i.BranchId == branchId);
            }

            // expose current filter values and branch names to the view for UI binding
            ViewData["q"] = q;
            ViewData["type"] = type ?? "all";
            ViewData["status"] = status ?? "all";
            ViewData["branch"] = branch ?? string.Empty;
            ViewData["Branches"] = _repo.GetAllBranches().ToList();

            return View(items.ToList());
        }
    }
}