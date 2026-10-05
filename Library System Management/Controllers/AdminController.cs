using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// AdminController.cs ()
// Controller for administrative functions: managing library catalog items (create, edit, delete).
// All actions are restricted to users in the "Admin" role via the Authorize attribute.
namespace Library_System_Management.Controllers
{
    // AdminController: allows administrators to manage catalog items (CRUD).
    // Actions are protected by the Admin role.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILibraryRepository _repo;

        public AdminController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // Details: view item details (admin view)
        public IActionResult Details(System.Guid id)
        {
            var it = _repo.GetItem(id);
            if (it == null) return NotFound();
            return View(it);
        }

        // Index: list all items
        public IActionResult Index()
        {
            var items = _repo.GetAllItems();
            return View(items);
        }

        // Show the create item form
        [HttpGet]
        public IActionResult Create() => View();

        // Handle create item POST. Accepts form fields and constructs the appropriate derived Item.
        [HttpPost]
        public IActionResult Create([FromForm] string type, [FromForm] string name, [FromForm] string libraryCode, [FromForm] string description,
            [FromForm] string? author, [FromForm] string? genre, [FromForm] int? pages,
            [FromForm] string? artist, [FromForm] int? year, [FromForm] string? format,
            [FromForm] string? toyType, [FromForm] int? recommendedAge)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Type and Name are required");
                return View();
            }

            Item item;
            switch (type.ToLowerInvariant())
            {
                case "book":
                    item = new Book { Name = name, LibraryCode = libraryCode ?? string.Empty, Description = description ?? string.Empty, Author = author ?? string.Empty, Genre = genre ?? string.Empty, Pages = pages ?? 0 };
                    break;
                case "music":
                    item = new Music { Name = name, LibraryCode = libraryCode ?? string.Empty, Description = description ?? string.Empty, Artist = artist ?? string.Empty, Year = year ?? 0, Format = format ?? "CD" };
                    break;
                case "toy":
                    item = new Toy { Name = name, LibraryCode = libraryCode ?? string.Empty, Description = description ?? string.Empty, ToyType = toyType ?? string.Empty, RecommendedAge = recommendedAge ?? 0 };
                    break;
                default:
                    ModelState.AddModelError("", "Unknown type");
                    return View();
            }

            _repo.AddItem(item);
            return RedirectToAction("Index");
        }

        // Edit: show the edit form for an item
        public IActionResult Edit(System.Guid id)
        {
            var it = _repo.GetItem(id);
            if (it == null) return NotFound();
            return View(it);
        }

        // Handle edit POST - updates basic item properties and status
        [HttpPost]
        public IActionResult Edit([FromForm] System.Guid id, [FromForm] string Name, [FromForm] string LibraryCode, [FromForm] string Description, [FromForm] string Status)
        {
            var it = _repo.GetItem(id);
            if (it == null) return NotFound();
            it.Name = Name;
            it.LibraryCode = LibraryCode;
            it.Description = Description;
            if (Enum.TryParse<Library_System_Management.Models.ItemStatus>(Status, out var st)) it.Status = st;
            _repo.UpdateItem(it);
            return RedirectToAction("Index");
        }

        // Delete: remove an item by id
        public IActionResult Delete(System.Guid id)
        {
            _repo.RemoveItem(id);
            return RedirectToAction("Index");
        }
    }
}
