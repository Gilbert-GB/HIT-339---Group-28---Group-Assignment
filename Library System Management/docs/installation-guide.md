# Installation Guide

## 1. Prerequisites

- **Windows** (the app uses SQL Server LocalDB, which is Windows-only)
- **.NET 10 SDK**
- **Visual Studio 2026** with the **ASP.NET and web development** workload. This workload includes **SQL Server Express LocalDB**, which the app requires for user accounts and roles.
  - If you use VS Code or the command line instead, install SQL Server Express LocalDB separately.

## 2. Clone the repository

```
git clone https://github.com/Gilbert-GB/HIT-339---Group-28---Group-Assignment.git
```

## 3. Open and run

**Visual Studio**
1. Open `Library System Management.slnx` (the solution file, not the folder).
2. Press **F5** (or the green Run button). NuGet packages restore automatically.

**Command line**
```
cd HIT-339---Group-28---Group-Assignment
dotnet restore
dotnet run --project "Library System Management/Library System Management.csproj"
```
Then open the HTTPS address shown in the terminal (for example `https://localhost:7xxx`).

## 4. What happens on first run

No manual database setup is needed.

- **The database is created automatically.** On startup the app applies its Entity Framework migrations to LocalDB, creating the database used for user accounts and roles.
- **Demo staff accounts are created automatically** (see section 5).
- **Catalogue data is held in memory.** Items, borrowers, loans, branches, reservations and notifications are seeded on every startup, so **any changes reset when the app restarts.** This is expected behaviour for the demo build.
- **Due-date reminders run automatically** on startup and then every hour.

## 5. Demo accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@library.local | P@ssw0rd1! |
| Reception | reception@library.local | P@ssw0rd1! |
| Manager | manager@library.local | P@ssw0rd1! |

Public pages (search, kiosk) need no login. Kiosk demo patrons include `alice@example.com`, `bob@example.com`, `david@example.com` and `eve@example.com`.

## 6. Configuration and testing the API

- **API key:** the read-only public API requires an `X-Api-Key` header matching `ApiKey` in `appsettings.json`. A demo key (`demo-library-api-key`) is preconfigured; change it for any real deployment.
- **Endpoints:**
  - `GET /api/Api/available`: available catalogue items
  - `GET /api/Api/categories`: item categories
  - `GET /api/Api/status`: opening status and branch list
- **Example** (replace the port with the one shown when the app starts):
```
  curl -k -H "X-Api-Key: demo-library-api-key" https://localhost:7xxx/api/Api/available
```
  Requests without the correct key return `401 Unauthorized`.
- **CSV import:** a sample file for testing the importer is in `docs/sample-import.csv`.

## 7. Running the tests

**Visual Studio:** Test > Run All Tests.

**Command line** (from the repository root):
```
dotnet test "Library System Management.slnx"
```

## 8. Troubleshooting

- **"A database operation failed" or "Cannot open database ... login failed"**
  A previous failed setup can leave a broken database registration. In Visual Studio, open **View > SQL Server Object Explorer**, expand `(localdb)\MSSQLLocalDB > Databases`, delete the `aspnet-Library_System_Management-...` database, and run the app again. It will be recreated.
- **Browser warns that the HTTPS certificate is not trusted**
  Run once: `dotnet dev-certs https --trust`
- **LocalDB not found**
  Install SQL Server Express LocalDB (included in the Visual Studio ASP.NET workload) and try again.