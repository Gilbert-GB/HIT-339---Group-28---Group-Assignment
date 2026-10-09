using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // KioskController: simple touch-friendly interface for in-library self-service kiosk.
    // Patrons identify themselves by email or library username, view an account summary
    // (loans, overdue items, fines, reservations), check out items by scanning or typing a
    // library code, and pay outstanding fines (simulated card payment).
    public class KioskController : Controller
    {
        private const int DefaultLoanDays = 14;
        private readonly ILibraryRepository _repo;

        public KioskController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        // GET: /Kiosk
        // Entry screen where a patron enters or scans their email or library username.
        public IActionResult Index() => View();

        // POST: /Kiosk/Lookup
        // Finds the patron's account and redirects to their account summary.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Lookup(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ViewData["Message"] = "Please enter or scan your email or library username.";
                return View("Index");
            }

            var borrower = FindBorrower(code);
            if (borrower == null)
            {
                ViewData["Message"] = "No account found for that code. Please see reception to register.";
                return View("Index");
            }

            return RedirectToAction(nameof(Account), new { id = borrower.Id });
        }

        // GET: /Kiosk/Account/{id}
        // Account summary: current loans (with overdue status and accruing fines), fines owing,
        // reservations, recent returns and total fines paid.
        public IActionResult Account(Guid id)
        {
            var borrower = _repo.GetBorrower(id);
            if (borrower == null) return RedirectToAction(nameof(Index));
            return View(BuildAccount(borrower));
        }

        // POST: /Kiosk/Checkout
        // Checks out an item by its library code for the patron whose account is open,
        // then returns to their account summary so they can see the new loan.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(Guid borrowerId, string itemCode, int days = DefaultLoanDays)
        {
            var borrower = _repo.GetBorrower(borrowerId);
            if (borrower == null) return RedirectToAction(nameof(Index));

            TryCheckout(borrower, itemCode, days);
            return RedirectToAction(nameof(Account), new { id = borrower.Id });
        }

        // POST: /Kiosk/CheckoutByIdentifier
        // Quick checkout from the Browse tab: identifies the patron by email or username.
        // Only existing accounts can borrow; new patrons are sent to reception to register.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckoutByIdentifier(string identifier, string itemCode, int days = DefaultLoanDays)
        {
            var borrower = string.IsNullOrWhiteSpace(identifier) ? null : FindBorrower(identifier);
            if (borrower == null)
            {
                TempData["KioskError"] = "No account found for that email or username. Please see reception to register.";
                return RedirectToAction(nameof(Index));
            }

            TryCheckout(borrower, itemCode, days);
            return RedirectToAction(nameof(Account), new { id = borrower.Id });
        }

        // GET: /Kiosk/Pay?borrowerId=...&recordId=...
        // Shows the simulated card payment screen for one outstanding fine.
        public IActionResult Pay(Guid borrowerId, Guid recordId)
        {
            var model = BuildPayment(borrowerId, recordId);
            if (model == null)
            {
                TempData["KioskError"] = "That fine could not be found or has already been paid.";
                return RedirectToAction(nameof(Account), new { id = borrowerId });
            }
            return View(model);
        }

        // POST: /Kiosk/Pay
        // Validates the card details and records a simulated payment. No real payment is processed,
        // and the full card number is never stored: only the last four digits are kept on the receipt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(Guid borrowerId, Guid recordId, string? cardName, string? cardNumber, string? expiry, string? cvv)
        {
            var model = BuildPayment(borrowerId, recordId);
            if (model == null)
            {
                TempData["KioskError"] = "That fine could not be found or has already been paid.";
                return RedirectToAction(nameof(Account), new { id = borrowerId });
            }

            var digits = new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            var cvvDigits = (cvv ?? string.Empty).Trim();

            string? error = null;
            if (string.IsNullOrWhiteSpace(cardName))
                error = "Please enter the name on the card.";
            else if (digits.Length != 16 || !PassesLuhnCheck(digits))
                error = "Please enter a valid 16-digit card number.";
            else if (!IsValidExpiry(expiry))
                error = "Please enter a valid expiry date (MM/YY) that is not in the past.";
            else if (cvvDigits.Length != 3 || !cvvDigits.All(char.IsDigit))
                error = "Please enter the 3-digit security code from the back of the card.";

            if (error != null)
            {
                model.Error = error;
                model.CardName = cardName;
                return View(model);
            }

            var record = _repo.PayFine(recordId, $"Card ending {digits[^4..]}");
            if (record == null)
            {
                TempData["KioskError"] = "That fine could not be paid. Please see reception.";
            }
            else
            {
                TempData["KioskSuccess"] = $"Payment of {record.FinePaid:C} received for '{model.ItemName}'. Your receipt number is {record.PaymentReference}.";
            }
            return RedirectToAction(nameof(Account), new { id = borrowerId });
        }

        // ---- Helpers ----

        private Borrower? FindBorrower(string code)
        {
            var c = code.Trim();
            return _repo.GetAllBorrowers().FirstOrDefault(b =>
                (!string.IsNullOrWhiteSpace(b.Email) && string.Equals(b.Email, c, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(b.Username) && string.Equals(b.Username, c, StringComparison.OrdinalIgnoreCase)));
        }

        // Attempts a checkout and records a clear success or error message for the patron.
        private void TryCheckout(Borrower borrower, string? itemCode, int days)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                TempData["KioskError"] = "Please enter or scan an item code.";
                return;
            }

            var code = itemCode.Trim().ToUpperInvariant();
            var item = _repo.GetItemByCode(code);
            if (item == null)
            {
                TempData["KioskError"] = $"No item found with code {code}.";
                return;
            }
            if (item.Status != ItemStatus.Available)
            {
                TempData["KioskError"] = $"'{item.Name}' is not available right now (status: {item.Status}).";
                return;
            }

            var record = _repo.BorrowItem(code, borrower, days);
            if (record == null)
            {
                TempData["KioskError"] = $"Unable to check out '{item.Name}'. Please see reception.";
                return;
            }

            TempData["KioskSuccess"] = $"Checked out '{item.Name}'. Due back {record.DueAt.ToLocalTime():d}.";
        }

        private KioskPaymentViewModel? BuildPayment(Guid borrowerId, Guid recordId)
        {
            var borrower = _repo.GetBorrower(borrowerId);
            var record = _repo.GetAllBorrowRecords().FirstOrDefault(r => r.Id == recordId);
            if (borrower == null || record == null || record.BorrowerId != borrowerId) return null;
            if (record.ReturnedAt == null || record.FinePaid <= 0 || record.FineSettled) return null;

            var item = _repo.GetItem(record.ItemId);
            return new KioskPaymentViewModel
            {
                BorrowerId = borrowerId,
                BorrowerName = borrower.FullName,
                RecordId = recordId,
                ItemName = item?.Name ?? "-",
                Code = item?.LibraryCode ?? "-",
                ReturnedAt = record.ReturnedAt,
                Amount = record.FinePaid
            };
        }

        // Luhn checksum: the standard check digit test used by real card numbers.
        private static bool PassesLuhnCheck(string digits)
        {
            var sum = 0;
            var doubleIt = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                var d = digits[i] - '0';
                if (doubleIt)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                doubleIt = !doubleIt;
            }
            return sum % 10 == 0;
        }

        // Accepts MM/YY or MM/YYYY; the card is valid until the end of its expiry month.
        private static bool IsValidExpiry(string? expiry)
        {
            if (string.IsNullOrWhiteSpace(expiry)) return false;
            var parts = expiry.Trim().Split('/');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year)) return false;
            if (month < 1 || month > 12) return false;
            if (year < 100) year += 2000;
            if (year > 9999) return false;
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return lastDay >= DateTime.Today;
        }

        private KioskAccountViewModel BuildAccount(Borrower borrower)
        {
            var now = DateTime.UtcNow;
            var branches = _repo.GetAllBranches().ToDictionary(b => b.Id, b => b.Name);
            string BranchOf(Item? item) =>
                item?.BranchId is Guid id && branches.TryGetValue(id, out var name) ? name : "-";

            var records = _repo.GetAllBorrowRecords().Where(r => r.BorrowerId == borrower.Id).ToList();

            var loans = records
                .Where(r => r.ReturnedAt == null)
                .OrderBy(r => r.DueAt)
                .Select(r =>
                {
                    var item = _repo.GetItem(r.ItemId);
                    var isOverdue = r.DueAt < now;
                    // same fine rule as returns: $1 per full day late
                    var daysLate = isOverdue ? (now - r.DueAt).Days : 0;
                    return new KioskLoanRow
                    {
                        Code = item?.LibraryCode ?? "-",
                        Name = item?.Name ?? "-",
                        Branch = BranchOf(item),
                        DueAt = r.DueAt,
                        IsOverdue = isOverdue,
                        DaysLate = daysLate,
                        AccruingFine = daysLate * 1.0m
                    };
                })
                .ToList();

            var outstandingFines = records
                .Where(r => r.ReturnedAt != null && r.FinePaid > 0 && !r.FineSettled)
                .OrderBy(r => r.ReturnedAt)
                .Select(r =>
                {
                    var item = _repo.GetItem(r.ItemId);
                    return new KioskFineRow
                    {
                        RecordId = r.Id,
                        Code = item?.LibraryCode ?? "-",
                        Name = item?.Name ?? "-",
                        ReturnedAt = r.ReturnedAt,
                        Amount = r.FinePaid
                    };
                })
                .ToList();

            var recentReturns = records
                .Where(r => r.ReturnedAt != null)
                .OrderByDescending(r => r.ReturnedAt)
                .Take(5)
                .Select(r =>
                {
                    var item = _repo.GetItem(r.ItemId);
                    return new KioskHistoryRow
                    {
                        Code = item?.LibraryCode ?? "-",
                        Name = item?.Name ?? "-",
                        ReturnedAt = r.ReturnedAt,
                        FineAmount = r.FinePaid,
                        FineSettled = r.FineSettled
                    };
                })
                .ToList();

            var reservations = new List<KioskReservationRow>();
            foreach (var res in _repo.GetAllReservations().Where(r => r.BorrowerId == borrower.Id))
            {
                var item = _repo.GetItem(res.ItemId);
                if (!res.Fulfilled)
                {
                    var queue = _repo.GetReservationsForItem(res.ItemId).Where(r => !r.Fulfilled).ToList();
                    var position = queue.FindIndex(r => r.Id == res.Id) + 1;
                    reservations.Add(new KioskReservationRow
                    {
                        Code = item?.LibraryCode ?? "-",
                        Name = item?.Name ?? "-",
                        Status = $"Waiting: #{position} in queue"
                    });
                }
                else if (item?.Status == ItemStatus.Available)
                {
                    reservations.Add(new KioskReservationRow
                    {
                        Code = item.LibraryCode,
                        Name = item.Name,
                        Status = "Ready to collect",
                        IsReady = true
                    });
                }
            }

            return new KioskAccountViewModel
            {
                Borrower = borrower,
                Loans = loans,
                OutstandingFines = outstandingFines,
                Reservations = reservations,
                RecentReturns = recentReturns,
                FinesPaid = records.Where(r => r.FineSettled).Sum(r => r.FinePaid)
            };
        }
    }
}