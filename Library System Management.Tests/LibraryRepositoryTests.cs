using System.Linq;
using Library_System_Management.Repositories;
using Xunit;

namespace Library_System_Management.Tests
{
    public class LibraryRepositoryTests
    {
        [Fact]
        public void Borrowing_Creates_Notification()
        {
            var repo = new InMemoryLibraryRepository();
            var borrower = repo.GetAllBorrowers().First();
            var available = repo.GetAllItems().First(i => i.Status == Library_System_Management.Models.ItemStatus.Available);
            var rec = repo.BorrowItem(available.LibraryCode, borrower, 7);
            Assert.NotNull(rec);
            var notes = repo.GetAllNotifications();
            Assert.Contains(notes, n => n.Type == "Borrowed" && n.Recipient.Contains(borrower.Email) );
        }

        [Fact]
        public void Returning_Notifies_Next_Reservation()
        {
            var repo = new InMemoryLibraryRepository();
            var item = repo.GetAllItems().First(i => i.Status == Library_System_Management.Models.ItemStatus.Available);
            var borrowers = repo.GetAllBorrowers().ToArray();
            var b1 = borrowers[0];
            var b2 = borrowers[1];
            // b1 borrows
            var rec = repo.BorrowItem(item.LibraryCode, b1, 7);
            Assert.NotNull(rec);
            // b2 reserves while borrowed
            repo.AddReservation(new Library_System_Management.Models.Reservation { ItemId = item.Id, BorrowerId = b2.Id });
            // return
            repo.ReturnItem(rec.Id);
            // notification for reservation should exist
            var notes = repo.GetAllNotifications();
            Assert.Contains(notes, n => n.Type == "ReservationAvailable" && n.Recipient.Contains(b2.Email) );
        }

        [Fact]
        public void TransferItem_Changes_Branch()
        {
            var repo = new InMemoryLibraryRepository();
            var item = repo.GetAllItems().First();
            var branches = repo.GetAllBranches().ToArray();
            var to = branches.Last();
            var ok = repo.TransferItem(item.Id, to.Id);
            Assert.True(ok);
            var it = repo.GetItem(item.Id);
            Assert.Equal(to.Id, it.BranchId);
        }

        [Fact]
        public void CsvImport_Adds_Items()
        {
            var csv = "LibraryCode,Type,Name\nX100,Book,Imported Book\nY200,Music,Imported Album";
            var repo = new InMemoryLibraryRepository();
            var (s,f) = repo.ImportItemsFromCsv(csv);
            Assert.Equal(2, s);
            Assert.Equal(0, f);
            var found = repo.GetItemByCode("X100");
            Assert.NotNull(found);
        }
    }
}
