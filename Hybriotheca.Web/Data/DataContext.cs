using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data;

public class DataContext : IdentityDbContext<AppUser>
{
    public DbSet<Edition> Editions { get; set; }

    public DbSet<BookStock> BooksInStock { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Work> Work { get; set; }

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

    }

}
