using System;

namespace Library_System_Management.Models
{
    // Item: abstract base class representing a catalog item in the library.
    // Concrete item types (Book, Music, Toy) inherit from this class.
    public abstract class Item
    {
        // Unique identifier for the item
        public Guid Id { get; set; } = Guid.NewGuid();

        // Human-friendly library code (e.g., B001, M001)
        public string LibraryCode { get; set; } = string.Empty;

        // Title or name of the item
        public string Name { get; set; } = string.Empty;

        // Optional description or notes
        public string Description { get; set; } = string.Empty;

        // Current status of the item (Available, Borrowed, etc.)
        public ItemStatus Status { get; set; } = ItemStatus.Available;
    }
}
