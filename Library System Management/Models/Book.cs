namespace Library_System_Management.Models
{
    // Book: represents a book in the catalog, extends Item with book-specific fields
    public class Book : Item
    {
        // Author of the book
        public string Author { get; set; } = string.Empty;
        // Genre/category
        public string Genre { get; set; } = string.Empty;
        // Number of pages (optional)
        public int Pages { get; set; }
    }
}
