using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // PublicController: handles public (anonymous) search and browsing of catalog items.
    // Supports query, type and status filters.
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
            return View(it);
        }

        // Index: public search page. Optional parameters:
        // - q: full-text query against name, description, or library code
        // - type: item type filter (book, music, toy, all)
        // - status: item status filter (Available, Borrowed, etc.)
        public IActionResult Index(string? q, string? type, string? status)
        {
            var items = _repo.GetAllItems();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                items = items.Where(i => i.Name.Contains(q, System.StringComparison.OrdinalIgnoreCase)
                    || i.Description.Contains(q ?? string.Empty, System.StringComparison.OrdinalIgnoreCase)
                    || i.LibraryCode.Contains(q, System.StringComparison.OrdinalIgnoreCase));
            }

            // filter by type if provided
            if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
            {
                var t = type.ToLowerInvariant();
                items = items.Where(i =>
                    (t == "book" && i is Models.Book) ||
                    (t == "music" && i is Models.Music) ||
                    (t == "toy" && i is Models.Toy)
                );
            }

            // filter by item status if provided
            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<Models.ItemStatus>(status, true, out var st))
                {
                    items = items.Where(i => i.Status == st);
                }
            }

            // expose current filter values to the view for UI binding
            ViewData["q"] = q;
            ViewData["type"] = type ?? "all";
            ViewData["status"] = status ?? "all";

            return View(items);
        }
    }
}
