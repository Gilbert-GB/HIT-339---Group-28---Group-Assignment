# Library System Management (Assignment 3)

This project is an ASP.NET Core MVC Library Management System extended for Assignment 3.

Prerequisites
- .NET 10 SDK
- Visual Studio 2026 or VS Code

Run
- Update connection string in appsettings.json if using SQL Server; the project uses an in-memory repository by default for demo.
- Build and run from Visual Studio or `dotnet run` in project folder.

API
- Read-only API endpoints (require X-Api-Key header matching configuration key `ApiKey` in appsettings.json):
  - GET /api/api/available - list available items
  - GET /api/api/categories - list item types
  - GET /api/api/status - operating status and branches

Kiosk
- Browse to /Kiosk for a touch-friendly in-library kiosk interface.

Reservations
- Public item details page allows placing a hold by providing your email. When an item is returned, the next patron in the queue is automatically notified.

Notifications
- Staff can view the notification log at /Notifications (Manager/Admin/Reception roles).

Import
- Admins/Managers can paste CSV content at /Import to bulk-import items. CSV header: LibraryCode,Type,Name

Branches
- Branch management and transfers are available at /Branches (Manager/Admin/Reception roles).

Exports
- Manager can export items and borrow counts to CSV at /Manager/ExportCsv

Docs
- See /docs for additional documentation and diagrams.

Activity Log
- See ActivityLog.md for a detailed log of changes and implementation notes.

Contact
- This project was extended by GitHub Copilot as part of an assignment. For issues, check ActivityLog.md and source files.
