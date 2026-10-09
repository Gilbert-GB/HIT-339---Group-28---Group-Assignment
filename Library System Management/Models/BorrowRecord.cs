using System;

namespace Library_System_Management.Models
{
    // BorrowRecord: represents a single borrow event linking an Item and a Borrower.
    // Records borrow/return timestamps, any fine assessed and whether it has been paid.
    public class BorrowRecord
    {
        // unique record id
        public Guid Id { get; set; } = Guid.NewGuid();

        // referenced item id (foreign key-like)
        public Guid ItemId { get; set; }

        // referenced borrower id
        public Guid BorrowerId { get; set; }

        // when the item was borrowed
        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;

        // due date for return
        public DateTime DueAt { get; set; }

        // optional returned timestamp
        public DateTime? ReturnedAt { get; set; }

        // fine amount assessed for this record (computed on return).
        // The name is kept for compatibility; the fine is only paid once FineSettled is true.
        public decimal FinePaid { get; set; }

        // payment details (simulated payments)
        public bool FineSettled { get; set; }
        public DateTime? FineSettledAt { get; set; }
        public string? PaymentMethod { get; set; }     // e.g. "Card ending 4242" (full card numbers are never stored)
        public string? PaymentReference { get; set; }  // receipt number
    }
}