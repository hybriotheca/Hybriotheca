using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data;

public class DataContext : IdentityDbContext<AppUser>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BookEdition> BookEditions { get; set; }
    public DbSet<BookStock> BooksInStock { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<Library> Libraries { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Model.GetEntityTypes()
        .SelectMany(s => s.GetForeignKeys())
        .Where(x => !x.GetConstraintName().Contains("FK_AspNet") || x.GetConstraintName() == "FK_AspNetUsers_Subscriptions_SubscriptionID")
        .ToList()
        .ForEach(x => x.DeleteBehavior = DeleteBehavior.Restrict);

        // Define literal names of Properties.
        var TotalStock = nameof(BookStock.TotalStock);
        var AvailableStock = nameof(BookStock.AvailableStock);

        modelBuilder.Entity<BookStock>()
            .ToTable(t => t.HasCheckConstraint(
                $"CK_{AvailableStock}_GreaterOrEqualZero",
                $"[{AvailableStock}] >= 0"))
            .ToTable(t => t.HasCheckConstraint(
                $"CK_{TotalStock}_GreaterOrEqual_{AvailableStock}",
                $"[{TotalStock}] >= [{AvailableStock}]"));

        // Define literal names of Properties.
        var MaxLoanDays = nameof(Subscription.MaxLoanDays);
        var MaxLoans = nameof(Subscription.MaxLoans);

        modelBuilder.Entity<Subscription>()
            .ToTable(t => t.HasCheckConstraint(
                $"CK_{MaxLoanDays}_GreaterOrEqualZero",
                $"[{MaxLoanDays}] >= 0"))
            .ToTable(t => t.HasCheckConstraint(
                $"CK_{MaxLoans}_GreaterOrEqualZero",
                $"[{MaxLoans}] >= 0"));
    }
}
