using System;

namespace Library_System_Management.Models
{
    // Notification: simulated notification record for email/SMS events
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Recipient { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty; // Email or SMS
        public string Type { get; set; } = string.Empty; // Borrowed, DueSoon, FineAccrued, ReservationAvailable
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed
    }
}
