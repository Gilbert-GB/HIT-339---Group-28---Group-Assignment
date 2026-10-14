# HIT339 – Group 28 – Library Management System

A web-based **Library Management System** developed for the HIT339 group assignment. The project extends an existing ASP.NET Core MVC library application with additional access channels, inventory management, reservations, reporting and integration features.

## Project overview

The application supports library patrons, reception staff, administrators and managers. It provides a searchable public catalogue, a self-service kiosk, borrowing and return workflows, multi-branch inventory management, and reporting tools.

**Technology:** ASP.NET Core MVC (.NET 10), C#, Git, and an in-memory repository.

> **Important:** The current implementation uses an in-memory data store rather than a persistent Entity Framework database. Data changes may not survive an application restart. Notifications are simulated rather than sent through an external messaging service.

## Features

| Area | Functionality |
| --- | --- |
| Public catalogue | Search by title, code, description or author; filter by item type and availability; view item details |
| Reservations | Place holds using an email address; first-in, first-out (FIFO) reservation queue; simulated notification on return |
| Self-service kiosk | Touch-friendly interface, account lookup, browse available items and quick checkout |
| Reception | Borrow and return items for patrons |
| Multi-branch inventory | Branch records, item-to-branch associations and branch transfers |
| Management | Operational statistics, notification view and CSV export of inventory and borrowing counts |
| Developer API | Read-only item, category and status information, protected with an API key |
| Import | Simple CSV/paste-based import workflow |
| Documentation | Installation guidance, user instructions and project activity records |

## Prerequisites

- Windows PC or virtual machine (VM)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- Visual Studio 2026, or Visual Studio Code with C# support
- A modern web browser

**SQL Server is not required** for the documented in-memory implementation.

## Installation and running instructions

The same procedure should be followed for each library-location Git branch.

### 1. Clone the repository

Open PowerShell and run:

```powershell
git clone https://github.com/Gilbert-GB/HIT-339---Group-28---Group-Assignment.git
cd "HIT-339---Group-28---Group-Assignment"
```

### 2. Select a branch

To run the main version:

```powershell
git checkout main
```

For a location-specific version, switch to its existing branch (if your team has created and pushed it):

```powershell
git fetch --all
git branch -a
git checkout location-central
```

Alternatively, create a new local branch for a location:

```powershell
git checkout -b location-central
```

Example location names used in the project documentation are `location-central`, `location-east` and `location-west`. **These are examples, not a guarantee that remote branches exist.**

### 3. Configure the API key for demonstration

For local development, set an `ApiKey` configuration value. For example, add the following entry to `Library System Management/appsettings.Development.json` within the JSON object:

```json
{
  "ApiKey": "demo-key-123"
}
```

Merge this property with any existing settings; do not replace the entire configuration file. Use environment variables or secret management for real deployments. **Do not commit production credentials.**

### 4. Restore, build and run

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project "Library System Management/Library System Management.csproj"
```

Alternatively, open the solution in Visual Studio and press **F5**.

### 5. Open the application

Open the **localhost URL printed in the terminal** when the application starts. The documentation gives `https://localhost:5001` as an example; the actual port may differ.

## Demonstration accounts

The project documentation identifies these seeded accounts in `Program.cs`:

| Role | Email | Demo password |
| --- | --- | --- |
| Administrator | `admin@library.local` | `P@ssw0rd1!` |
| Reception | `reception@library.local` | `P@ssw0rd1!` |
| Manager | `manager@library.local` | `P@ssw0rd1!` |

> These are demonstration credentials only. Change them before any public deployment.

## How to use the system

### Public catalogue and reservations

1. Open **Home → Search**.
2. Search by title, library code, description or author.
3. Filter by **Type** (Books, Music or Toys) and **Status** (Available, Borrowed or Damaged).
4. Open an item to view details and its branch location.
5. If needed, place a hold using an email address. Reservations are processed in FIFO order.

### Self-service kiosk

1. Navigate to `/Kiosk`.
2. Use **Lookup** to find an existing account by email or username and view outstanding items.
3. Use **Browse Items** to select an available item.
4. Enter an email or username in the quick-checkout dialog and confirm.
5. Check for the on-screen confirmation.

### Reception workflow

1. Sign in with the reception demonstration account.
2. Open **Reception → Borrow** to issue an item using its code.
3. Use **Return** to register a returned item.
4. When a reserved item is returned, the system records a simulated notification for the next patron.

### Manager workflow

1. Sign in with the manager demonstration account.
2. Open **Manager → Index** to review statistics.
3. Use the branch management functionality to view or transfer inventory.
4. Export item-level information and borrowing counts using **Export CSV**.
5. Review simulated messages through **Notifications**.

## Multi-location demonstration

The application models multiple library branches. Two demonstrations are described in the project documentation:

**Option A — Git branch per location:** Configure a different default branch in each location-specific Git branch, then build and run that branch using the same commands above. The relevant seed data is in:

```text
Library System Management/Repositories/InMemoryLibraryRepository.cs
```

The documented approach uses the `seededBranches` collection and the `defaultBranchId` selection to determine the default branch. For example, select a different index for a different location, then commit that configuration to the appropriate Git branch.

**Option B — Inventory transfers:** Use the branch transfer interface to move items between library branches and demonstrate branch-specific inventory.

> Location-specific Git branches and default selections should be verified against the submitted repository before marking.

## Developer API

The project includes a **read-only API** for external integrations, exposing information about available items, categories and status. Requests require an `X-Api-Key` header.

Consult the application's **Developer → API Docs** page for the actual endpoint paths and request examples. This README does not assume endpoint URLs that were not specified in the supplied documentation.

## Implementation status and limitations

**Documented as implemented:** Public kiosk, read-only API with API-key check, simulated notifications, multi-branch inventory and transfer UI, FIFO reservations, CSV import, CSV export and supporting documentation.

**Not implemented / partial according to the supplied documentation:**

- Persistent Entity Framework-backed storage (current repository is in-memory).
- Background service for automatic due-soon reminders and notifications.

## Project documentation

The project documentation references a `docs/` directory, including:

- `docs/continuation-guide.md` — development handover and continuation guidance.
- Additional requirements and user-journey documentation.
- `ActivityLog.md` — implementation activity record (refer to the repository for its actual location).

Team meeting minutes, team contract, task allocation and demonstration video should be supplied with the assignment where required. Their completed contents and links must be provided by the team rather than assumed here.

## Suggested demonstration sequence

1. Public catalogue search, filtering and reservation.
2. Kiosk account lookup and quick checkout.
3. Reception borrowing and returns, including reservation notification.
4. Manager statistics, branch inventory and CSV export.
5. Developer API documentation and API-key requirement.

**Target video length:** approximately 4–5 minutes.

## Assignment information

- **Unit:** HIT339
- **Group:** 28
- **Project:** Library Management System
- **Repository:** [Gilbert-GB/HIT-339---Group-28---Group-Assignment](https://github.com/Gilbert-GB/HIT-339---Group-28---Group-Assignment)

*Prepared from the group's supplied project documentation. Check actual branch names, project paths, demo credentials and feature behaviour against the final submitted codebase before submission.*
