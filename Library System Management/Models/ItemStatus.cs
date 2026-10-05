namespace Library_System_Management.Models
{
    // ItemStatus: Represents the lifecycle/status of a library item.
    public enum ItemStatus
    {
        // Item is available for borrowing
        Available,
        // Item is currently borrowed by a borrower
        Borrowed,
        // Item is damaged and may not be lendable
        Damaged,
        // Item is marked for destruction / removed from catalog
        Destroy
    }
}
