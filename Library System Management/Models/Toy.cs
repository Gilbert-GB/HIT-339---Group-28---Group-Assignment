namespace Library_System_Management.Models
{
    // Toy: represents a toy entry with type and recommended minimum age
    public class Toy : Item
    {
        // Category or toy type (e.g., Building, Puzzle)
        public string ToyType { get; set; } = string.Empty;
        // Recommended minimum age for the toy
        public int RecommendedAge { get; set; }
    }
}
