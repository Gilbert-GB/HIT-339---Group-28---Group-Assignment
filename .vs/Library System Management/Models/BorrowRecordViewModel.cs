using System;

namespace Library_System_Management.Models
{
    // BorrowRecordViewModel: used to display friendly borrow record information in UI
    // Includes item name/code and borrower display name/username for readability.
    public class BorrowRecordViewModel
    {
        public Guid Id { get; set; }
        public Guid BorrowRecordId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BorrowerUsername { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateTime DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public decimal FinePaid { get; set; }
    }
}
