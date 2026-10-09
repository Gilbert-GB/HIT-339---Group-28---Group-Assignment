namespace Library_System_Management.Services
{
    // ExternalItemRecord: one catalogue record returned by an external metadata provider.
    public class ExternalItemRecord
    {
        public string ExternalId { get; set; } = string.Empty; // ISBN for books, catalogue number for music
        public string Type { get; set; } = string.Empty;       // "Book" or "Music"
        public string Title { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;    // author or artist
        public string Category { get; set; } = string.Empty;   // genre for books, format for music
        public int? Year { get; set; }                         // release year (music)
    }

    // IMetadataProvider: abstraction over an external book/media metadata service.
    // The mock implementation below could be replaced by a real API client
    // (for example Open Library) without changing the Import controller.
    public interface IMetadataProvider
    {
        string Name { get; }
        IEnumerable<ExternalItemRecord> Search(string query);
        ExternalItemRecord? GetById(string externalId);
    }

    // MockMetadataProvider: simulates an external catalogue API with a fixed set of sample records.
    public class MockMetadataProvider : IMetadataProvider
    {
        public string Name => "Mock Open Catalogue";

        private static readonly List<ExternalItemRecord> Records = new()
        {
            new() { ExternalId = "9780441013593", Type = "Book", Title = "Dune", Creator = "Frank Herbert", Category = "Science Fiction" },
            new() { ExternalId = "9780345391803", Type = "Book", Title = "The Hitchhiker's Guide to the Galaxy", Creator = "Douglas Adams", Category = "Science Fiction" },
            new() { ExternalId = "9780060850524", Type = "Book", Title = "Brave New World", Creator = "Aldous Huxley", Category = "Dystopian" },
            new() { ExternalId = "9780439023481", Type = "Book", Title = "The Hunger Games", Creator = "Suzanne Collins", Category = "Young Adult" },
            new() { ExternalId = "9780062316097", Type = "Book", Title = "Sapiens", Creator = "Yuval Noah Harari", Category = "History" },
            new() { ExternalId = "9780735211292", Type = "Book", Title = "Atomic Habits", Creator = "James Clear", Category = "Self-Help" },
            new() { ExternalId = "9780134757599", Type = "Book", Title = "Refactoring", Creator = "Martin Fowler", Category = "Programming" },
            new() { ExternalId = "9780062315007", Type = "Book", Title = "The Alchemist", Creator = "Paulo Coelho", Category = "Fiction" },
            new() { ExternalId = "CAT-1001", Type = "Music", Title = "Nevermind", Creator = "Nirvana", Category = "CD", Year = 1991 },
            new() { ExternalId = "CAT-1002", Type = "Music", Title = "Back to Black", Creator = "Amy Winehouse", Category = "Vinyl", Year = 2006 },
            new() { ExternalId = "CAT-1003", Type = "Music", Title = "Purple Rain", Creator = "Prince and the Revolution", Category = "Vinyl", Year = 1984 },
            new() { ExternalId = "CAT-1004", Type = "Music", Title = "Random Access Memories", Creator = "Daft Punk", Category = "Digital", Year = 2013 },
            new() { ExternalId = "CAT-1005", Type = "Music", Title = "To Pimp a Butterfly", Creator = "Kendrick Lamar", Category = "Digital", Year = 2015 }
        };

        // Search: matches an ISBN or catalogue number (hyphens and spaces ignored),
        // or any part of the title or creator.
        public IEnumerable<ExternalItemRecord> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<ExternalItemRecord>();

            var q = query.Trim();
            var compact = q.Replace("-", "").Replace(" ", "");

            return Records.Where(r =>
                r.ExternalId.Replace("-", "").Equals(compact, StringComparison.OrdinalIgnoreCase) ||
                r.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.Creator.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public ExternalItemRecord? GetById(string externalId) =>
            Records.FirstOrDefault(r => r.ExternalId.Equals(externalId, StringComparison.OrdinalIgnoreCase));
    }
}