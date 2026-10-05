# Continuation Guide for Assignment 3 Work

Prepared by: GitHub Copilot

Purpose
-------
This document explains what was implemented, where the code lives, and clear next steps so a teammate can pick up the work and continue development safely.

High-level summary of changes implemented
-----------------------------------------
- Domain: Added Branch, Reservation, Notification models and wired them into the in-memory repository.
- Repository: Extended ILibraryRepository and InMemoryLibraryRepository to support branches, transfers, reservations, notifications, and a simple CSV importer.
- Kiosk: Added a dedicated kiosk UI (Views/Kiosk) with a simplified kiosk layout, lookup and browse tabs, clickable sample items, and a quick-checkout modal. Kiosk creates lightweight borrowers when checking out (CheckoutByIdentifier).
- Public API: Added ApiController (read-only endpoints: /api/api/available, /api/api/categories, /api/api/status). API key enforced via configuration key `ApiKey`.
- Notifications: Simulated notifications saved in-memory; NotificationsController and view display a log for staff.
- Reservations: PublicController.Reserve lets patrons place holds; ReturnItem marks reservations fulfilled and generates notification for next patron.
- Branches & Transfers: BranchesController and Views/Branches/Index provide branch listing and transfer UI; items were seeded to a default branch.
- CSV Importer: ImportController and Views/Import/Index allow admins to paste CSV content and import items using InMemoryLibraryRepository.ImportItemsFromCsv.
- Manager: ManagerController extended with ExportCsv action; Manager/Index improved with quick actions panel.
- UI/UX: Centralized and improved site.css, added toast notifications, icons, and a consistent navbar for the kiosk.
- Tests: Added a basic xUnit test project (Library System Management.Tests) with repository tests for key workflows.

Key files and where to look first
--------------------------------
- Models:
  - Models/Branch.cs, Models/Reservation.cs, Models/Notification.cs, Models/Item.cs (BranchId added)
- Data / DI:
  - Data/ApplicationDbContext.cs (DbSets added for new models)
  - Program.cs (registers InMemoryLibraryRepository)
- Repository:
  - Repositories/ILibraryRepository.cs (new methods)
  - Repositories/InMemoryLibraryRepository.cs (core logic, seeding, notifications, reservation handling, CSV import)
- Controllers & Views (UI):
  - Controllers/KioskController.cs, Views/Kiosk/*
  - Controllers/ApiController.cs
  - Controllers/NotificationsController.cs, Views/Notifications/Index.cshtml
  - Controllers/BranchesController.cs, Views/Branches/Index.cshtml
  - Controllers/ImportController.cs, Views/Import/Index.cshtml
  - Controllers/ManagerController.cs (ExportCsv), Views/Manager/Index.cshtml
  - Views/Shared/_Layout.cshtml, Views/Shared/_KioskLayout.cshtml, Views/Shared/_Toasts.cshtml
- Docs & activity:
  - docs/* and ActivityLog.md

How to build and run (developer workstation)
--------------------------------------------
1. Ensure .NET 10 SDK is installed.
2. Open the solution in Visual Studio 2026 or run from terminal:
   - dotnet restore
   - dotnet build
   - dotnet run --project "Library System Management/Library System Management.csproj"
3. Default in-memory repository seeds sample data on startup. Demo users are created in Program.cs:
   - admin@library.local / P@ssw0rd1! (Admin)
   - reception@library.local / P@ssw0rd1! (Reception)
   - manager@library.local / P@ssw0rd1! (Manager)

Testing
-------
- Run unit tests:
  - dotnet test
- Manual checks (important flows):
  - Kiosk: /Kiosk lookup by seeded emails (alice@example.com, bob@example.com) and quick-checkout via clickable items.
  - Public reserve: /Public/Details/{id} place a hold and verify notification after return.
  - Reception borrow/return flows (Reception role) and verify Notifications page updated.
  - Branch transfer: /Branches and Manager CSV export (/Manager/ExportCsv).

Design & coding notes
---------------------
- The repository is currently in-memory (InMemoryLibraryRepository). To persist data, implement ILibraryRepository using ApplicationDbContext and EF Core; move seeding logic into a data seeder that runs only in development.
- Notifications are simulated and stored in the in-memory dictionary inside InMemoryLibraryRepository. If persisted, add a Notification DbSet and update controllers accordingly.
- API key: The ApiController checks configuration key `ApiKey`. Do not hardcode secrets — use appsettings or environment variables for deployment.

Suggested next tasks (ordered, with references)
----------------------------------------------
1. Persist repository to EF Core (HIGH):
   - Create EfLibraryRepository implementing ILibraryRepository.
   - Add migrations for Branches, Reservations, Notifications, Items, Borrowers, BorrowRecords.
   - Update Program.cs to conditionally register the EF implementation.
   - Files: Repositories/*, Data/ApplicationDbContext.cs

2. Add due-soon and fine-accrued notification generator (MED):
   - Implement a background worker (IHostedService) that scans upcoming due dates and creates Notification entries.
   - Files: new Services/DueNotificationService.cs, register in Program.cs

3. Improve CSV importer (MED):
   - Support file upload (IFormFile) and better validation per column, return detailed import summary.
   - Files: Controllers/ImportController.cs, Views/Import/Index.cshtml, Repositories/InMemoryLibraryRepository.cs

4. Secure API key management (LOW-MED):
   - Add a secure Admin UI to rotate/check the API key (store in secure configuration or secret store).
   - Files: new Controllers/SettingsController.cs, Views/Settings/*

5. UI polish and accessibility (LOW):
   - Add keyboard focus, larger touch targets, and ARIA attributes to kiosk controls.

Developer tips & conventions
--------------------------
- Stick to existing patterns: controllers call repository; keep logic out of views.
- Use dependency injection for new services and register them in Program.cs.
- Keep UI changes isolated in Views/ and style adjustments in wwwroot/css/site.css.
- When adding persistence, write migration scripts and update ActivityLog.md with steps.

Where to get help / debug notes
------------------------------
- If build fails, run `dotnet build` and inspect errors. Many view compile errors come from Razor variable scoping — avoid duplicate var names in the same Razor scope.
- For Git issues, remove .vs artifacts and ensure .gitignore has `.vs/` (already added).

Contact
-------
If anything is unclear, open an issue in the repo or contact the original implementer. Leave notes in ActivityLog.md when you change behavior.
