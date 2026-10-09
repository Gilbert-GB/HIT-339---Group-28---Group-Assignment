# Testing Summary

## Automated tests

The solution includes an xUnit test project, `Library System Management.Tests`, containing repository tests (`LibraryRepositoryTests.cs`). The project was originally missing from the solution file, so the tests did not run; it has since been added. All tests pass.

Run them in Visual Studio (Test > Run All Tests) or from the repository root with:

```
dotnet test "Library System Management.slnx"
```

## Manual tests

All manual tests were performed on a fresh clone with an empty database, using the seeded demo data.

### Setup and deployment
- First run on a fresh machine: database created automatically, demo staff accounts seeded, home page loads with no manual steps.
- Login as Admin, Reception and Manager: each role reaches its own area; other areas show Access Denied.

### Borrowing, returns and reservations
- Borrow via Reception: borrow record created, item set to Borrowed, notification logged.
- Return via Reception: record closed, fine calculated when late, item set to Available.
- Reservation queue (FIFO): two patrons queued for The Hobbit; on return, only the first patron (Eve) was notified.
- Reservation on a damaged item: Admin changed Thriller from Damaged to Available; the waiting patron (Carol) was notified.
- Public reserve: reservation placed by email; duplicate reservations prevented.
- Holds can only be placed on Borrowed or Damaged items; Available and removed items show an explanation instead. The queue length counts only patrons still waiting, and a duplicate hold shows the patron's existing position.

### Notifications
- On startup, a due-soon email (Monopoly) and a fine-accruing SMS (Rubik's Cube, 13 days late, $13.00) were created automatically.
- "Run due-date check" immediately afterwards created no duplicate reminders.

### Kiosk
- Account summary for an overdue patron: overdue item highlighted, days late and accruing fine shown, summary totals correct.
- Reservations shown with queue position (#1 and #2 for The Hobbit).
- Checkout by library code: loan added, patron stays on their account with a confirmation.
- Checkout of a damaged item: refused with a clear message.
- Quick checkout with an unknown email: refused, patron directed to reception.
- Inactivity: account screen returned to the start screen after 90 seconds.
- Fine payment: invalid card number rejected; valid test card accepted; receipt number issued; fine marked Paid on the account; FinePaid receipt logged in Notifications; manager dashboard and fine revenue audit show collected and outstanding totals correctly.

### Import
- CSV upload of `docs/sample-import.csv`: 3 rows imported; 4 rejected with line-specific reasons (unknown type, wrong code prefix, duplicate code, unknown branch).
- Quoted value containing a comma ("Thinking, Fast and Slow") imported correctly.
- External lookup: found by title and by ISBN with hyphens; imported with the next free library code; a second import of the same record was refused.

### Search
- Search by title, author ("Tolkien") and library code.
- Branch filter returns only that branch's items; branch shown on each result.
- Result count displays correctly.

### Branches
- Item transferred between branches; new branch shown in search results and reports.

### Manager reports
- All four CSV reports downloaded and opened in Excel; totals (borrows, outstanding loans, fines) match the dashboard; branch breakdown and items needing attention correct.

### API
- `GET /api/Api/available` with a valid `X-Api-Key` header: JSON list of available items.
- Same request without the key: `401 Unauthorized`.
- `GET /api/Api/status` in a browser: opening status and branch list returned without a key.

## Known limitations found during testing
- Catalogue data resets when the application restarts (in-memory storage).
- A reserved item that becomes available is not held for the notified patron.
- The home page image does not load, and staff links appear in the navigation when logged out (the pages themselves remain protected).