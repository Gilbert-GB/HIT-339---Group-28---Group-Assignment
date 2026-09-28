using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Library_System_Management.Models;

namespace Library_System_Management.Repositories
{
    public class InMemoryLibraryRepository : ILibraryRepository
    {
        private readonly ConcurrentDictionary<Guid, Item> _items = new();
        private readonly ConcurrentDictionary<Guid, Borrower> _borrowers = new();
        private readonly ConcurrentDictionary<Guid, BorrowRecord> _records = new();

        // InMemoryLibraryRepository: demo implementation that keeps data in memory.
        // It seeds a collection of sample items, borrowers and borrow records to operate without a database.
        public InMemoryLibraryRepository()
        {
            // seed with many items, borrowers, and borrow records for demo/testing
            // seed canonical lists and extend them programmatically to reach 30 items each for a richer demo dataset
            var books = new List<Book> {
                new Book{LibraryCode="B001", Name="The Odyssey", Author="Homer", Genre="Epic", Pages=300},
                new Book{LibraryCode="B002", Name="C# in Depth", Author="Jon Skeet", Genre="Programming", Pages=900},
                new Book{LibraryCode="B003", Name="Clean Code", Author="Robert C. Martin", Genre="Programming", Pages=464},
                new Book{LibraryCode="B004", Name="The Pragmatic Programmer", Author="Andrew Hunt", Genre="Programming", Pages=352},
                new Book{LibraryCode="B005", Name="To Kill a Mockingbird", Author="Harper Lee", Genre="Fiction", Pages=281},
                new Book{LibraryCode="B006", Name="1984", Author="George Orwell", Genre="Dystopian", Pages=328},
                new Book{LibraryCode="B007", Name="The Great Gatsby", Author="F. Scott Fitzgerald", Genre="Fiction", Pages=180},
                new Book{LibraryCode="B008", Name="Moby Dick", Author="Herman Melville", Genre="Adventure", Pages=635},
                new Book{LibraryCode="B009", Name="War and Peace", Author="Leo Tolstoy", Genre="Historical", Pages=1225},
                new Book{LibraryCode="B010", Name="Pride and Prejudice", Author="Jane Austen", Genre="Romance", Pages=279},
                new Book{LibraryCode="B011", Name="Effective Java", Author="Joshua Bloch", Genre="Programming", Pages=416},
                new Book{LibraryCode="B012", Name="Design Patterns", Author="Erich Gamma", Genre="Programming", Pages=395},
                new Book{LibraryCode="B013", Name="Introduction to Algorithms", Author="Cormen et al.", Genre="Computer Science", Pages=1312},
                new Book{LibraryCode="B014", Name="The Hobbit", Author="J.R.R. Tolkien", Genre="Fantasy", Pages=310},
                new Book{LibraryCode="B015", Name="The Catcher in the Rye", Author="J.D. Salinger", Genre="Fiction", Pages=214}
            };
            // generate additional books until we have 30
            for (int i = books.Count + 1; i <= 30; i++)
            {
                books.Add(new Book
                {
                    LibraryCode = $"B{i:000}",
                    Name = $"Sample Book {i}",
                    Author = $"Author {i}",
                    Genre = "General",
                    Pages = 100 + i
                });
            }

            var music = new List<Music> {
                new Music{LibraryCode="M001", Name="Kind of Blue", Artist="Miles Davis", Year=1959, Format="Vinyl"},
                new Music{LibraryCode="M002", Name="Abbey Road", Artist="The Beatles", Year=1969, Format="CD"},
                new Music{LibraryCode="M003", Name="Thriller", Artist="Michael Jackson", Year=1982, Format="CD"},
                new Music{LibraryCode="M004", Name="Back in Black", Artist="AC/DC", Year=1980, Format="Vinyl"},
                new Music{LibraryCode="M005", Name="Rumours", Artist="Fleetwood Mac", Year=1977, Format="CD"},
                new Music{LibraryCode="M006", Name="The Wall", Artist="Pink Floyd", Year=1979, Format="Vinyl"},
                new Music{LibraryCode="M007", Name="Hotel California", Artist="Eagles", Year=1976, Format="CD"},
                new Music{LibraryCode="M008", Name="21", Artist="Adele", Year=2011, Format="Digital"},
                new Music{LibraryCode="M009", Name="Blue Train", Artist="John Coltrane", Year=1957, Format="Vinyl"},
                new Music{LibraryCode="M010", Name="The Beatles (White Album)", Artist="The Beatles", Year=1968, Format="Vinyl"}
            };
            for (int i = music.Count + 1; i <= 30; i++)
            {
                music.Add(new Music
                {
                    LibraryCode = $"M{i:000}",
                    Name = $"Sample Album {i}",
                    Artist = $"Artist {i}",
                    Year = 2000 + (i % 23),
                    Format = (i % 3) == 0 ? "Digital" : ((i % 2) == 0 ? "CD" : "Vinyl")
                });
            }

            var toys = new List<Toy> {
                new Toy{LibraryCode="T001", Name="Lego Set", ToyType="Building", RecommendedAge=8},
                new Toy{LibraryCode="T002", Name="Rubik's Cube", ToyType="Puzzle", RecommendedAge=8},
                new Toy{LibraryCode="T003", Name="Barbie Doll", ToyType="Doll", RecommendedAge=5},
                new Toy{LibraryCode="T004", Name="Remote Car", ToyType="Electronic", RecommendedAge=7},
                new Toy{LibraryCode="T005", Name="Play-Doh", ToyType="Creative", RecommendedAge=3},
                new Toy{LibraryCode="T006", Name="Monopoly", ToyType="Board Game", RecommendedAge=8},
                new Toy{LibraryCode="T007", Name="Tonka Truck", ToyType="Vehicle", RecommendedAge=4},
                new Toy{LibraryCode="T008", Name="Action Figure", ToyType="Figure", RecommendedAge=6},
                new Toy{LibraryCode="T009", Name="Steam Kit", ToyType="Educational", RecommendedAge=10},
                new Toy{LibraryCode="T010", Name="Chess Set", ToyType="Board Game", RecommendedAge=8}
            };
            for (int i = toys.Count + 1; i <= 30; i++)
            {
                toys.Add(new Toy
                {
                    LibraryCode = $"T{i:000}",
                    Name = $"Sample Toy {i}",
                    ToyType = (i % 4) == 0 ? "Educational" : ((i % 3) == 0 ? "Puzzle" : "Figure"),
                    RecommendedAge = 3 + (i % 12)
                });
            }

            // add all items
            foreach(var b in books) AddItem(b);
            foreach(var m in music) AddItem(m);
            foreach(var t in toys) AddItem(t);

            // seed borrowers
            var borrowers = new[] {
                new Borrower{FullName="Alice Johnson", Email="alice@example.com", Phone="555-0101"},
                new Borrower{FullName="Bob Smith", Email="bob@example.com", Phone="555-0202"},
                new Borrower{FullName="Carol Lee", Email="carol@example.com", Phone="555-0303"},
                new Borrower{FullName="David Kim", Email="david@example.com", Phone="555-0404"},
                new Borrower{FullName="Eve Torres", Email="eve@example.com", Phone="555-0505"},
                new Borrower{FullName="Frank Wright", Email="frank@example.com", Phone="555-0606"}
            };
            foreach(var b in borrowers) AddBorrower(b);

            // create some borrow records (some returned, some overdue, some active)
            void AddRecord(Item item, Borrower borrower, DateTime borrowedAt, int days, DateTime? returnedAt=null)
            {
                item.Status = returnedAt == null ? ItemStatus.Borrowed : ItemStatus.Available;
                UpdateItem(item);
                var rec = new BorrowRecord { ItemId = item.Id, BorrowerId = borrower.Id, BorrowedAt = borrowedAt, DueAt = borrowedAt.AddDays(days), ReturnedAt = returnedAt };
                if (returnedAt.HasValue && returnedAt.Value > rec.DueAt)
                {
                    rec.FinePaid = (returnedAt.Value - rec.DueAt).Days * 1.0m;
                }
                _records[rec.Id] = rec;
            }

            // active borrow: Alice borrowed C# in Depth 3 days ago for 14 days
            var bookCSharp = GetItemByCode("B002");
            var alice = _borrowers.Values.FirstOrDefault(x => x.Email == "alice@example.com");
            if (bookCSharp != null && alice != null) AddRecord(bookCSharp, alice, DateTime.UtcNow.AddDays(-3), 14, null);

            // overdue borrow: Bob borrowed The Odyssey 30 days ago for 14 days, returned late
            var odyssey = GetItemByCode("B001");
            var bob = _borrowers.Values.FirstOrDefault(x => x.Email == "bob@example.com");
            if (odyssey != null && bob != null) AddRecord(odyssey, bob, DateTime.UtcNow.AddDays(-30), 14, DateTime.UtcNow.AddDays(-10));

            // active music borrow: Carol borrowed M002 1 day ago
            var abbey = GetItemByCode("M002");
            var carol = _borrowers.Values.FirstOrDefault(x => x.Email == "carol@example.com");
            if (abbey != null && carol != null) AddRecord(abbey, carol, DateTime.UtcNow.AddDays(-1), 7, null);

            // overdue toy borrow: David borrowed T002 20 days ago for 7 days, not yet returned
            var rubik = GetItemByCode("T002");
            var david = _borrowers.Values.FirstOrDefault(x => x.Email == "david@example.com");
            if (rubik != null && david != null) AddRecord(rubik, david, DateTime.UtcNow.AddDays(-20), 7, null);

            // returned borrow with small fine: Eve borrowed B010 25 days ago for 7 days, returned 5 days late
            var pride = GetItemByCode("B010");
            var eve = _borrowers.Values.FirstOrDefault(x => x.Email == "eve@example.com");
            if (pride != null && eve != null) AddRecord(pride, eve, DateTime.UtcNow.AddDays(-25), 7, DateTime.UtcNow.AddDays(-18));

            // a few more random records
            var frank = _borrowers.Values.FirstOrDefault(x => x.Email == "frank@example.com");
            var misc1 = GetItemByCode("B014");
            if (misc1 != null && frank != null) AddRecord(misc1, frank, DateTime.UtcNow.AddDays(-2), 10, null);

            var misc2 = GetItemByCode("M005");
            if (misc2 != null && alice != null) AddRecord(misc2, alice, DateTime.UtcNow.AddDays(-40), 14, DateTime.UtcNow.AddDays(-20));

            var misc3 = GetItemByCode("T006");
            if (misc3 != null && bob != null) AddRecord(misc3, bob, DateTime.UtcNow.AddDays(-5), 7, null);

        }

        // AddItem: add or replace an item in the in-memory collection.
        public void AddItem(Item item)
        {
            _items[item.Id] = item;
        }

        // GetAllItems: return all items sorted by name.
        public IEnumerable<Item> GetAllItems() => _items.Values.OrderBy(i => i.Name);

        // GetItem: find an item by its GUID identifier.
        public Item? GetItem(Guid id) => _items.TryGetValue(id, out var it) ? it : null;

        // GetItemByCode: find an item by the human-friendly library code.
        public Item? GetItemByCode(string code) => _items.Values.FirstOrDefault(i => i.LibraryCode.Equals(code, StringComparison.OrdinalIgnoreCase));

        // RemoveItem: remove an item by id.
        public void RemoveItem(Guid id)
        {
            _items.TryRemove(id, out _);
        }

        // UpdateItem: replace item state in the collection.
        public void UpdateItem(Item item)
        {
            _items[item.Id] = item;
        }

        public BorrowRecord? BorrowItem(string code, Borrower borrower, int days)
        {
            var item = GetItemByCode(code);
            if (item == null || item.Status != ItemStatus.Available) return null;

            // ensure borrower exists
            // try matching borrower by email, then username, then full name
            Borrower? existing = null;
            if (!string.IsNullOrWhiteSpace(borrower.Email))
                existing = _borrowers.Values.FirstOrDefault(b => string.Equals(b.Email, borrower.Email, StringComparison.OrdinalIgnoreCase));
            if (existing == null && !string.IsNullOrWhiteSpace(borrower.Username))
                existing = _borrowers.Values.FirstOrDefault(b => string.Equals(b.Username, borrower.Username, StringComparison.OrdinalIgnoreCase));
            if (existing == null && !string.IsNullOrWhiteSpace(borrower.FullName))
                existing = _borrowers.Values.FirstOrDefault(b => string.Equals(b.FullName, borrower.FullName, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                AddBorrower(borrower);
                existing = borrower;
            }

            item.Status = ItemStatus.Borrowed;
            UpdateItem(item);

            var rec = new BorrowRecord
            {
                ItemId = item.Id,
                BorrowerId = existing.Id,
                BorrowedAt = DateTime.UtcNow,
                DueAt = DateTime.UtcNow.AddDays(days)
            };
            _records[rec.Id] = rec;
            return rec;
        }

        // ReturnItem: mark a borrow record as returned, set the item status back to Available
        // and compute a simple daily fine if the return is late.
        public void ReturnItem(Guid borrowRecordId)
        {
            if (!_records.TryGetValue(borrowRecordId, out var rec)) return;
            if (_items.TryGetValue(rec.ItemId, out var item))
            {
                item.Status = ItemStatus.Available;
                UpdateItem(item);
            }
            rec.ReturnedAt = DateTime.UtcNow;
            // calculate simple fine: $1 per day late
            if (rec.ReturnedAt > rec.DueAt)
            {
                var daysLate = (rec.ReturnedAt.Value - rec.DueAt).Days;
                if (daysLate < 0) daysLate = 0;
                rec.FinePaid = daysLate * 1.0m;
            }
            _records[rec.Id] = rec;
        }

        // Borrower operations
        public IEnumerable<Borrower> GetAllBorrowers() => _borrowers.Values.OrderBy(b => b.FullName);

        public Borrower? GetBorrower(Guid id) => _borrowers.TryGetValue(id, out var b) ? b : null;

        // AddBorrower: add or replace borrower
        public void AddBorrower(Borrower b)
        {
            _borrowers[b.Id] = b;
        }

        // UpdateBorrower: update borrower details
        public void UpdateBorrower(Borrower b)
        {
            _borrowers[b.Id] = b;
        }

        public void RemoveBorrower(Guid id)
        {
            _borrowers.TryRemove(id, out _);
        }

        public IEnumerable<BorrowRecord> GetAllBorrowRecords() => _records.Values.OrderByDescending(r => r.BorrowedAt);
    }
}
