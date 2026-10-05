using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // KioskController: simple touch-friendly interface for in-library self-service kiosk
    public class KioskController : Controller
    {
        private readonly ILibraryRepository _repo;

        public KioskController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // GET: /Kiosk
        // Simple entry screen where a patron can enter/scan their library code (username/email)
        public IActionResult Index() => View();

        // POST: lookup account by provided code and display account summary
        [HttpPost]
        public IActionResult Lookup(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return View("Index", "Please enter your library code.");
            var borrowers = _repo.GetAllBorrowers();
            var b = borrowers.FirstOrDefault(x => string.Equals(x.Username, code, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Email, code, StringComparison.OrdinalIgnoreCase));
            if (b == null)
            {
                ViewData["Message"] = "No account found for that code. Please contact reception.";
                return View("Index");
            }

            // gather outstanding borrow records for the borrower
            var records = _repo.GetAllBorrowRecords().Where(r => r.BorrowerId == b.Id && r.ReturnedAt == null);
            ViewData["Borrower"] = b;
            return View("Account", records);
        }

        // POST: attempt to checkout an item from kiosk for a borrower
        [HttpPost]
        public IActionResult Checkout(string borrowerId, string itemCode, int days = 14)
        {
            if (string.IsNullOrWhiteSpace(borrowerId) || string.IsNullOrWhiteSpace(itemCode)) return BadRequest();
            if (!Guid.TryParse(borrowerId, out var bid)) return BadRequest();
            var borrower = _repo.GetBorrower(bid);
            if (borrower == null) return NotFound();

            var rec = _repo.BorrowItem(itemCode.Trim(), borrower, days);
            if (rec == null)
            {
                TempData["KioskError"] = "Unable to borrow the requested item. It may be unavailable.";
            }
            else
            {
                TempData["KioskSuccess"] = "Item borrowed successfully.";
            }
            return RedirectToAction("Index");
        }

        // NOTE: kiosk registration removed; account creation should be managed by staff or separate flow.

        // POST: Checkout by identifier (email or username) and item code - used by kiosk quick-checkout
        [HttpPost]
        public IActionResult CheckoutByIdentifier(string identifier, string itemCode, int days = 14)
        {
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(itemCode))
            {
                TempData["KioskError"] = "Identifier and item code are required.";
                return RedirectToAction("Index");
            }

            // Try find borrower by email or username
            Borrower? borrower = _repo.GetAllBorrowers().FirstOrDefault(b =>
                (!string.IsNullOrWhiteSpace(b.Email) && string.Equals(b.Email, identifier, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(b.Username) && string.Equals(b.Username, identifier, StringComparison.OrdinalIgnoreCase)));

            if (borrower == null)
            {
                // create a lightweight borrower record for quick kiosk registration
                borrower = new Borrower { Email = identifier, FullName = identifier };
                _repo.AddBorrower(borrower);
            }

            var rec = _repo.BorrowItem(itemCode.Trim(), borrower, days);
            if (rec == null)
            {
                TempData["KioskError"] = "Unable to borrow the requested item. It may be unavailable or code is invalid.";
            }
            else
            {
                TempData["KioskSuccess"] = $"Item {itemCode.Trim()} borrowed successfully for {borrower.FullName}.";
            }

            return RedirectToAction("Index");
        }
    }
}
