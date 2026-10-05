using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library_System_Management.Models;

namespace Library_System_Management.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        // Add DbSets for new Assignment 3 domain models so EF-aware projects can use them.
        public DbSet<Branch>? Branches { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<Notification>? Notifications { get; set; }
    }
}
