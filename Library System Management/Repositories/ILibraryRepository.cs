using System;
using System.Collections.Generic;
using Library_System_Management.Models;

namespace Library_System_Management.Repositories
{
    // ILibraryRepository: defines repository operations required by the app.
    // Implementations may use in-memory collections (demo) or a database (EF Core).
    public interface ILibraryRepository
    {
        // Items
        IEnumerable<Item> GetAllItems();
        Item? GetItem(Guid id);
        Item? GetItemByCode(string code);
        void AddItem(Item item);
        void UpdateItem(Item item);
        void RemoveItem(Guid id);

        // Borrowers
        IEnumerable<Borrower> GetAllBorrowers();
        Borrower? GetBorrower(Guid id);
        void AddBorrower(Borrower b);
        void UpdateBorrower(Borrower b);
        void RemoveBorrower(Guid id);

        // Borrowing operations
        // BorrowItem: attempts to borrow the item with the given library code for the provided borrower and duration.
        BorrowRecord? BorrowItem(string code, Borrower borrower, int days);
        // ReturnItem: mark a borrow record as returned and calculate fine if applicable.
        void ReturnItem(Guid borrowRecordId);

        // Borrow records
        IEnumerable<BorrowRecord> GetAllBorrowRecords();

        // Branches (multi-branch support)
        IEnumerable<Branch> GetAllBranches();
        Branch? GetBranch(Guid id);
        void AddBranch(Branch b);
        // Transfer an item to another branch
        bool TransferItem(Guid itemId, Guid toBranchId);

        // Reservations / waitlist
        IEnumerable<Reservation> GetReservationsForItem(Guid itemId);
        void AddReservation(Reservation r);
        IEnumerable<Reservation> GetAllReservations();

        // Notifications (simulated email/SMS)
        IEnumerable<Notification> GetAllNotifications();
        void AddNotification(Notification n);

        // Simple CSV import summary
        (int success, int failed) ImportItemsFromCsv(string csv);
    }
}
