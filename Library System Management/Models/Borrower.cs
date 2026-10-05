using System;

namespace Library_System_Management.Models
{
    public class Borrower
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        // full name of the borrower
        public string FullName { get; set; } = string.Empty;
        // optional username/display name used for friendly references
        public string Username { get; set; } = string.Empty;
        // email address (used to identify borrowers as well)
        public string Email { get; set; } = string.Empty;
        // phone number or contact
        public string Phone { get; set; } = string.Empty;
    }
}
