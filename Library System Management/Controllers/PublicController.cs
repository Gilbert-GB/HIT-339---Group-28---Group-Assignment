using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // PublicController: handles public (anonymous) search and browsing of catalog items.
    // Supports query, type, status and branch filters, item details and reservations.
    public class PublicController : Controller
    {
        private readonly ILibraryRepository _repo;

        public PublicController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // Details: show full details for a single item, its branch and its waiting list
        public IActionResult Details(System.Guid id)
        {
            var it = _repo.GetItem(id);
            if (it == null) return NotFound();

            // only patrons still waiting count towards the queue (notified patrons are fulfilled)
            ViewData["ReservationCount"] = _repo.GetReservationsForItem(id).Count(r => !r.Fulfilled);
            ViewData["BranchName"] = it.BranchId.HasValue ? _repo.GetBranch(it.BranchId.Value)?.Name : null;
            return View(it);
        }

        // POST: Place a reservation (hold) on an item that is currently Borrowed or Damaged.
        // Accepts the item id and an email to identify the patron.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reserve(Guid id, string email)
        {
            var item = _repo.GetItem(id);
            if (item == null) return NotFound();

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ReservationMessage"] = "Please enter your email address to place a hold.";
                return RedirectToAction("Details", new { id });
            }

            // holds are only for items that cannot be borrowed right now but will return
            if (item.Status != ItemStatus.Borrowed && item.Status != ItemStatus.Damaged)
            {
                TempData["ReservationMessage"] = item.Status == ItemStatus.Available
                    ? "This item is available now, so no hold is needed. Visit the library or use a kiosk to borrow it."
                    : "This item has been removed from circulation and cannot be reserved.";
                return RedirectToAction("Details", new { id });
            }

            // find or create borrower by email
            email = email.Trim();
            var borrower = _repo.GetAllBorrowers().FirstOrDefault(b => string.Equals(b.Email, email, StringComparison.OrdinalIgnoreCase));
            if (borrower == null)
            {
                borrower = new Borrower { Email = email, FullName = email };
                _repo.AddBorrower(borrower);
            }

            // tell the patron if they are already waiting, rather than silently ignoring the request
            var waiting = _repo.GetReservationsForItem(id).Where(r => !r.Fulfilled).ToList();
            var existingIndex = waiting.FindIndex(r => r.BorrowerId == borrower.Id);
            if (existingIndex >= 0)
            {
                TempData["ReservationMessage"] = $"You are already on the waiting list for this item (position {existingIndex + 1}).";
                return RedirectToAction("Details", new { id });
            }

            _repo.AddReservation(new Reservation { ItemId = id, BorrowerId = borrower.Id });

            TempData["ReservationMessage"] = $"Your hold has been placed. You are number {waiting.Count + 1} on the waiting list, and we will notify you when the item becomes available.";
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