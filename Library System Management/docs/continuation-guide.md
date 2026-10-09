# Continuation Guide for Assignment 3 Work

Prepared by: Gilbert (initial implementation, prepared with GitHub Copilot) and Abdul (fixes and feature completion)

Purpose
-------
This document explains what has been implemented, where the code lives, and what remains, so a teammate can pick up the work safely.

Current status
--------------
All Part B modules from the brief are implemented and working:

| Brief requirement | Where it lives | Status |
|---|---|---|
| Self-service kiosk | `/Kiosk` | Done: account lookup, account summary, checkout by library code, quick checkout from Browse |
| Public API | `/api/Api/...` | Done: `available` and `categories` (API key), `status` (public) |
| Multi-channel notifications | `/Notifications` | Done: borrowed, due soon (Email), fine accruing (SMS), reservation available; automated hourly |
| Multi-branch inventory | `/Branches`, search filter | Done: 3 seeded branches, item transfers, branch shown in search and reports |
| Reservation waitlist | Public item details, `/Kiosk` | Done: FIFO queue for borrowed or damaged items; next patron notified when the item becomes Available |
| Data importer | `/Import` | Done: CSV file upload or paste with validation report; mock external metadata provider (ISBN/title/artist lookup) |
| Manager analytics and export | `/Manager` | Done: four CSV reports (borrowing statistics, fine revenue audit, inventory health, items and borrow counts) |

Still outstanding (see "Suggested next tasks"): per-location installation instructions, and the Part A documentation.

Changes by Gilbert (initial implementation)
-------------------------------------------
- Domain: Branch, Reservation and Notification models, wired into the in-memory repository.
- Repository: ILibraryRepository and InMemoryLibraryRepository extended for branches, transfers, reservations, notifications and CSV import.
- Kiosk UI, public API, notification log, reservations, branches and transfers, initial CSV importer, initial manager CSV export.
- UI/UX: centralised site.css, toasts, icons, kiosk layout.
- xUnit test project with repository tests.

Changes by Abdul
----------------
Fixes
- `Program.cs`: migrations apply automatically on startup. Previously the app failed on a fresh machine because the Identity database was never created, and the seeding try/catch hid the error.
- `SyncModel` migration added for the Branch, Notification and Reservation DbSets in ApplicationDbContext. These tables are not used yet (data lives in memory), but without the migration .NET 10 refuses to apply migrations at all.
- Test project added to the solution file (it existed but was not included). All tests pass.
- Public search: fixed the result count, which displayed "1 ?? 0 items" due to a Razor rendering bug.
- `appsettings.json`: demo API key added. Without one, every API request returned 401.
- API status endpoint made public, so the "API Status" nav link works in a browser.

Features
- Manager reports: borrowing statistics, fine revenue audit and inventory health CSV exports; fixed CSV escaping in the existing export.
- Seed data: items spread across all three branches; damaged and destroyed items; seeded reservations for the FIFO demo.
- Reservations: the next patron is notified whenever an item becomes Available (returns and admin repairs), via `UpdateItem` in the repository.
- Notifications: `Services/DueDateNotificationService.cs` (background service, runs on startup then hourly) plus a "Run due-date check" button. Due-soon reminders go by Email, fine-accruing alerts by SMS. Reminders are not duplicated.
- Importer: `Services/CsvItemImporter.cs` (file upload, header-based columns, line-by-line validation) and `Services/MockMetadataProvider.cs` (behind `IMetadataProvider`, so a real API could replace it). Sample file: `docs/sample-import.csv`.
- Public search: branch on each result, branch filter, author and artist search.
- Kiosk: account summary with overdue status, accruing fines, fines paid, reservations with queue position and recent returns; stays on the account after checkout; Done button and 90-second inactivity sign-out; staff links and dead code removed; quick checkout requires an existing account.
- Installation guide rewritten (LocalDB requirement, automatic setup, API testing, tests, troubleshooting).

Key files
---------
- Models: `Item.cs` (BranchId), `Branch.cs`, `Reservation.cs`, `Notification.cs`, `KioskAccountViewModel.cs`, `ImportViewModel.cs`
- Data: `Data/ApplicationDbContext.cs`, `Data/Migrations/*`
- Repository: `Repositories/ILibraryRepository.cs`, `Repositories/InMemoryLibraryRepository.cs` (seeding, borrowing, returns, reservation queue, due-date notifications)
- Services: `DueDateNotificationService.cs`, `CsvItemImporter.cs`, `MockMetadataProvider.cs`
- Controllers: `KioskController`, `ApiController`, `NotificationsController`, `BranchesController`, `ImportController`, `ManagerController`, `PublicController`
- Startup: `Program.cs`

How to build and run
--------------------
See `docs/installation-guide.md`. In short: open `Library System Management.slnx` in Visual Studio and press F5. The database is created automatically; demo data is held in memory and resets on every restart.

Demo accounts (all passwords `P@ssw0rd1!`): `admin@library.local`, `reception@library.local`, `manager@library.local`.

Manual test checklist
---------------------
- Kiosk: `/Kiosk`, look up `david@example.com` (overdue item with accruing fine), `eve@example.com` and `bob@example.com` (#1 and #2 in the queue for The Hobbit). Check out an item by code.
- Reservations: as Reception, return The Hobbit; `/Notifications` shows a message to Eve only. As Admin, change Thriller from Damaged to Available; Carol is notified.
- Notifications: on startup, a due-soon email (Monopoly, Bob) and a fine-accruing SMS (Rubik's Cube, David) appear. "Run due-date check" creates no duplicates.
- Importer: upload `docs/sample-import.csv` as Admin; expect 3 imported and 4 rejected with reasons. Look up `Dune` or ISBN `978-0-441-01359-3` and import it.
- Manager: download all four CSV reports; totals match the dashboard.
- Search: filter by branch; search "Tolkien" (author search).
- API: `curl.exe -s -k -H "X-Api-Key: demo-library-api-key" https://localhost:<port>/api/Api/available` returns JSON; without the header returns 401.

Design and coding notes
-----------------------
- Data is held in `InMemoryLibraryRepository` (singleton). Anything that changes item status should go through `UpdateItem`, which also triggers the reservation queue.
- ApplicationDbContext includes DbSets for Branch, Reservation and Notification. Any change to these models (or new DbSets) requires a new migration, or the app will fail to start (`PendingModelChangesWarning`).
- Notifications are simulated: they are logged in memory and shown on `/Notifications`; nothing is actually sent.
- The API key is read from configuration (`ApiKey`). The demo key is for coursework only.

Known limitations
-----------------
- Catalogue data resets on restart (in-memory storage).
- When a reserved item becomes Available, the waiting patron is notified, but the item is not held for them; another patron could borrow it first.
- The home page image is broken, and staff links (Admin, Borrowing, Manager) show in the nav when logged out (they are still access-protected).
- API routes are `/api/Api/...` (doubled "api") because the controller is named `ApiController`.

Suggested next tasks
--------------------
1. Per-location installation (HIGH, required by the brief): a way for each library location to run the same install and choose its branch, for example a `LocationBranch` setting in `appsettings.json` or a git branch per location. Must be tested on the VM.
2. Part A documentation (HIGH): project plan, team contracts, billable hours, meeting minutes, user guide, diagrams.
3. Small UI fixes (LOW): broken home image, hide staff nav links when logged out, point "API Status" at an API documentation page.
4. Persist data with EF Core (LOW, optional): implement ILibraryRepository with ApplicationDbContext and swap the registration in `Program.cs`. The brief prioritises breadth over deep refactoring.

Developer tips
--------------
- Follow the existing pattern: controllers call the repository; keep logic out of views.
- Register new services in `Program.cs`.
- Before committing, test on a fresh clone with an empty database.
- Record changes in `ActivityLog.md`.

Where to get help / debug notes
------------------------------
- If build fails, run `dotnet build` and inspect errors. Many view compile errors come from Razor variable scoping — avoid duplicate var names in the same Razor scope.
- For Git issues, remove .vs artifacts and ensure .gitignore has `.vs/` (already added).

Contact
-------
If anything is unclear, open an issue in the repo or contact the original implementer. Leave notes in ActivityLog.md when you change behavior.
