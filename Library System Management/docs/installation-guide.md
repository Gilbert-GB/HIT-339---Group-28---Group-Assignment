# Installation Guide

1. Prerequisites
   - .NET 10 SDK
   - Visual Studio 2026 (recommended) or VS Code

2. Clone repository
   - git clone https://github.com/Gilbert-GB/HIT-339---Group-28---Group-Assignment.git

3. Open solution in Visual Studio and restore NuGet packages.

4. Configuration
   - The project uses an in-memory demo repository by default. If you want to use SQL Server, update `appsettings.json` connection string and ensure ApplicationDbContext is used for repository implementation.
   - Optionally set `ApiKey` in appsettings.json for API access.

5. Run
   - Press F5 or `dotnet run` from project directory.

6. Demo accounts
   - admin@library.local / P@ssw0rd1! (Admin)
   - reception@library.local / P@ssw0rd1! (Reception)
   - manager@library.local / P@ssw0rd1! (Manager)

7. Notes
   - Data seeded by the in-memory repository resets when the application restarts.
