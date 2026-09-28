# Activity Log — Library System Management

This file records the implementation activities performed in this workspace while building a simple Library Management System. The log is chronological and comprehensive so you can track what was done and where to verify it.

Prepared by: GitHub Copilot
Date: 2026-08-12 (updated)

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

If you want, I can implement any of the recommended next steps now — tell me which one and I will apply the changes.

*** End of log
