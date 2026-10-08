namespace Library_System_Management.Models
{
    // KioskAccountViewModel: everything the kiosk account summary screen shows
    // for one patron: current loans, fines, reservations and recent returns.
    public class KioskAccountViewModel
    {
        public Borrower Borrower { get; set; } = new();
        public List<KioskLoanRow> Loans { get; set; } = new();
        public List<KioskReservationRow> Reservations { get; set; } = new();
        public List<KioskHistoryRow> RecentReturns { get; set; } = new();
        public decimal FinesPaid { get; set; }

        public int OverdueCount => Loans.Count(l => l.IsOverdue);
        public decimal FinesAccruing => Loans.Sum(l => l.AccruingFine);
    }

    public class KioskLoanRow
    {
        public string Code { get; set; } = "-";
        public string Name { get; set; } = "-";
        public string Branch { get; set; } = "-";
        public DateTime DueAt { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysLate { get; set; }
        public decimal AccruingFine { get; set; }
    }

    public class KioskReservationRow
    {
        public string Code { get; set; } = "-";
        public string Name { get; set; } = "-";
        public string Status { get; set; } = string.Empty;
        public bool IsReady { get; set; }
    }

    public class KioskHistoryRow
    {
        public string Code { get; set; } = "-";
        public string Name { get; set; } = "-";
        public DateTime? ReturnedAt { get; set; }
        public decimal FinePaid { get; set; }
    }
}