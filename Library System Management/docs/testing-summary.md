# Testing Summary

Manual tests performed:
- Borrowing an item via Reception: BorrowRecord created, item status set to Borrowed, notification logged.
- Returning an item via Reception: BorrowRecord returned, fine calculated when late, reservation queue checked and next patron notified if exists.
- Public reserve: Placed a reservation for an item using an email; duplicate reservations prevented.
- Kiosk: Lookup by email/username and checkout an available item.
- Branch transfer: Transferred an item between seeded branches and verified branch assignment.
- CSV import: Pasted simple CSV and verified new items added to catalogue.
- Manager export: Exported items report to CSV.

Automated tests: None added yet. Recommended to add unit tests for repository flows and controller endpoints.
