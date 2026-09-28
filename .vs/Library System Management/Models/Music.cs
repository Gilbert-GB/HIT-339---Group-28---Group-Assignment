namespace Library_System_Management.Models
{
    // Music: represents a music record (album, single) with artist/year/format info
    public class Music : Item
    {
        // Performing artist or band
        public string Artist { get; set; } = string.Empty;
        // Release year
        public int Year { get; set; }
        // Format (CD, Vinyl, Digital)
        public string Format { get; set; } = "CD";
    }
}
