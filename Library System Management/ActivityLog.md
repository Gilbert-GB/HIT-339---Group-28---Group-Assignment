# Activity Log — Library System Management

This file records the implementation activities performed in this workspace while building a simple Library Management System. The log is chronological and comprehensive so you can track what was done and where to verify it.

Prepared by: GitHub Copilot
Date: 05-10-2026 (updated)

---

## Summary of delivered features

- Domain models: Item (base), Book, Music, Toy; Borrower (with Username), BorrowRecord.
- In-memory repository (ILibraryRepository + InMemoryLibraryRepository) with a large seeded dataset (books, music, toys, borrowers, borrow records).
- Authentication and roles (Identity): Admin, Reception, Manager. Demo seeded users created at startup.
- Admin CRUD for items, Reception borrow/return and borrower management, Manager statistics, Public responsive search with filters.
- UI polishing: landing page, global site.css, admin-specific admin.css and admin.js for type-specific fields (available for reuse).
- Activity log (this file) documenting all changes and verification steps.

---

## Full chronological log (what was done)

1) Project and environment
   - Confirmed ASP.NET Core project targeting .NET 10.
   - Kept default Identity and ApplicationDbContext present.

2) Models
   - Added Item base class and ItemStatus enum.
   - Added Book, Music, Toy derived classes with type-specific properties.
   - Added Borrower model (FullName, Username, Email, Phone).
   - Added BorrowRecord to track borrows, due dates, returns and fines.
   - Added BorrowRecordViewModel to present friendly data in Reception views.

3) Repository
   - Implemented ILibraryRepository interface for required operations.
   - Implemented InMemoryLibraryRepository with thread-safe ConcurrentDictionary storage.
   - Seeded many items (B001–B015, M001–M010, T001–T010), borrowers and borrow records (active, overdue, returned).
   - BorrowItem matches borrowers by Email, then Username, then FullName to avoid duplicates.
   - ReturnItem computes a $1/day fine if returned late.

4) Controllers & Views (MVC)
   - AdminController + Views/Admin: item list, Create, Edit, Delete (original implementation preserved).
   - ReceptionController + Views/Reception: borrow workflow, return action, borrower CRUD, EditBorrower added for username changes.
   - ManagerController + Views/Manager: statistics view.
   - PublicController + Views/Public: public responsive search with q/type/status filters and mobile-friendly UI.

5) Identity & Roles
   - Registered Identity with AddRoles<IdentityRole>().
   - Seeded roles (Admin, Reception, Manager) and demo users in Program.cs startup scope; assigned roles and EmailConfirmed=true.
   - Protected controllers with [Authorize(Roles = "...")] for Admin/Reception/Manager.

6) Admin Razor Pages (experiment) and reversion
   - Implemented Admin as Razor Pages (Pages/Admin/Index/Create/Edit/Delete) with admin.css and admin.js.
   - Later, per user request, removed those Razor Pages and restored the original MVC AdminController and Views/Admin.
   - Kept admin.css and admin.js available in wwwroot for optional reuse.

7) UI and style improvements
   - Reworked site.css with a professional theme: hero, feature cards, CTA, compact tables.
   - Replaced Home/Index with a polished landing page (hero, search, CTA, quick links).
   - Added admin.css and admin.js for admin page improvements (type-specific field toggling).
   - Modified Views/Shared/_Layout.cshtml to support @RenderSection("Styles") for page-specific CSS injection.

8) Search & filters
   - Public search supports query (q), type (book/music/toy/all), and status (Available/Borrowed/Damaged/Destroy/all).
   - PublicController.Apply filters server-side and passes current filter values to the view.
   - Public view updated with dropdown filters and preserved selections in query string.

9) Borrower features
   - Added Username field to borrower forms (CreateBorrower, Borrow forms).
   - Reception added EditBorrower view/controller to update Username and other details.
   - BorrowItem attempts to find an existing borrower by Email -> Username -> FullName before creating new borrower.

10) Borrow records and display
   - Reception Index initially showed GUIDs for items/borrowers; created BorrowRecordViewModel and updated the view to show ItemName, ItemCode, BorrowerName, BorrowerUsername and friendly dates and fines.

11) Debugging & environment
   - Helped resolve Visual Studio/IIS Express "same key" duplication issue by removing .vs folder and bin/obj as needed.
   - Executed dotnet dev-certs https --trust and guided trust steps for local HTTPS.

12) Additional features
   - Seeded dataset is significantly expanded for testing and demo purposes.
   - Added Export/Bulk ideas (not implemented) as potential next steps.

---

## Assignment 3 additions (horizontal feature expansion)

- Added multi-branch support: Branch model, assigned seeded items to branches, BranchesController and Views/Branches/Index for branch inventory and transfers.
- Added Reservation model and public reservation endpoint (PublicController.Reserve). Prevent duplicate reservations and maintain FIFO queue. When an item is returned the next patron is automatically identified and a notification is generated.
- Added Notification model and a NotificationsController with a staff-facing log view at /Notifications to show simulated email/SMS messages for events (borrowed, reservation available, fines).
- Integrated notifications into BorrowItem and ReturnItem so borrowing generates a Borrowed notification and returning triggers ReservationAvailable notifications.
- Added Kiosk module: KioskController and simple touch-friendly views at /Kiosk for in-library self-service (lookup by email/username and checkout items).
- Added simple read-only API (ApiController) with endpoints for available items, categories, and operating status using an API key via X-Api-Key header.
- Added CSV importer (ImportController + view) allowing admins to paste CSV content to bulk-create items; in-memory importer validates rows and returns a summary.
- Added Manager CSV export (ManagerController.ExportCsv) to export item inventory and borrow counts by branch.

All additions are implemented additively and reuse existing repository and controller patterns to avoid breaking Assignment 2 functionality.

---

## Verification checklist

- Admin: sign in as admin@library.local / P@ssw0rd1!, open Admin → Items, create/edit/delete items.
- Reception: sign in as reception@library.local / P@ssw0rd1!, create borrowers, edit Username, borrow items by LibraryCode, return items and check fines.
- Manager: sign in as manager@library.local / P@ssw0rd1!, view Manager → Index for counts and fine totals.
- Public search: open Home or Public → Search, try q/type/status filters on mobile and desktop.

---

## Files added or significantly changed (high level)

- Models: Item.cs, ItemStatus.cs, Book.cs, Music.cs, Toy.cs, Borrower.cs, BorrowRecord.cs, BorrowRecordViewModel.cs
- Repositories: ILibraryRepository.cs, InMemoryLibraryRepository.cs (large seed)
- Controllers: AdminController.cs, ReceptionController.cs, ManagerController.cs, PublicController.cs
- Views: Admin/*, Reception/* (Borrow, Borrowers, EditBorrower), Manager/Index.cshtml, Public/Index.cshtml, Home/Index.cshtml (landing)
- Pages (temporary): Pages/Admin/* (later removed per preference)
- wwwroot/css/site.css, wwwroot/css/admin.css, wwwroot/js/admin.js
- Program.cs: DI registration, Identity role seeding
- ActivityLog.md (this file)

---

## Recommended next steps (pick one or more)

1. Persist data: migrate InMemoryLibraryRepository to EF Core (ApplicationDbContext) and create migrations so data persists across runs.
2. Convert Reception and Manager to Razor Pages for full Razor-style project consistency.
3. Add Admin user-management UI (create users, change roles) or scaffold Identity UI for production readiness.
4. Add unit/integration tests for core workflows (borrow/return, fines) and for controllers/repository.
5. Add borrower self-service: authenticated borrower profile page to set Username (requires Identity integration for borrowers or linking borrower records to Identity users).

---

## Abdul's changes (October 2026)

Fixes
- Program.cs: migrations now apply automatically on startup. Previously the app failed on a fresh machine because the Identity database was never created, and seeding errors were hidden by a try/catch.
- Added SyncModel migration covering the Branch, Notification and Reservation DbSets (unused groundwork for EF persistence; without it, .NET 10 refused to apply migrations).
- Added the existing test project to the solution file (it was not included, so the tests never ran). All tests pass.
- Public search: fixed the result count ("1 ?? 0 items" was a Razor rendering bug).
- appsettings.json: added a demo API key. Without one, every API request returned 401.
- API: the status endpoint is now public (no key). The "API Status" nav link opened it in the browser, which cannot send the key header, so it always showed a 401 error. Catalogue endpoints remain key-protected.

Features
- Manager exports: borrowing statistics, fine revenue audit and inventory health reports (CSV), plus fixed CSV escaping in the existing export.
- Seed data: items spread across all three branches; damaged and destroyed items; seeded reservations (FIFO queue demo).
- Reservations: the next patron is now notified whenever an item becomes Available, including admin repairs, not only returns.
- Notifications: automated "due soon" (Email) and "fine accruing" (SMS) reminders via a background service (startup, then hourly), plus a manual "Run due-date check" button. Reminders are never duplicated.
- Importer: CSV file upload with line-by-line validation (type, code prefix, duplicates, branch, numbers); mock external metadata provider (search by ISBN, title or artist; one-click import with automatic code); sample file in docs/sample-import.csv.
- Public search: branch shown on results, branch filter, author and artist search.
- Item details: holds restricted to Borrowed or Damaged items (per the brief); queue length counts only waiting patrons; duplicate holds show the patron's position; branch shown; anti-forgery protection on the hold form.
- Kiosk: account summary (overdue status, accruing fines, fines paid, reservations with queue position, recent returns); stays on the account after checkout; Done button and 90-second inactivity sign-out; removed staff links and dead code; quick checkout requires an existing account.

Documentation
- Rewrote the installation guide (LocalDB required, automatic database setup, in-memory data note, API testing, tests, troubleshooting).
- Rewrote the user guide with a patron section on search, filters, reservations and the kiosk, plus a staff quick reference.
- Updated the testing summary and the continuation guide (current status, known limitations, next tasks).

Verification
- See the manual test checklist in docs/continuation-guide.md and the results in docs/testing-summary.md.

---

*** End of log