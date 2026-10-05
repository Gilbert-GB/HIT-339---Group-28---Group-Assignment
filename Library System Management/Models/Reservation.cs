using System;

namespace Library_System_Management.Models
{
    // Reservation: represents a hold placed by a borrower on an item
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public Guid BorrowerId { get; set; }
        // when the reservation was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // whether the reservation has been fulfilled (notified)
        public bool Fulfilled { get; set; } = false;
    }
}
