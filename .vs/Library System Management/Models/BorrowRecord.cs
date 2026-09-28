using System;

namespace Library_System_Management.Models
{
    // BorrowRecord: represents a single borrow event linking an Item and a Borrower
    // Records borrow/return timestamps and any fines paid.
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

        // monetary fine paid for this record (computed on return)
        public decimal FinePaid { get; set; }
    }
}
