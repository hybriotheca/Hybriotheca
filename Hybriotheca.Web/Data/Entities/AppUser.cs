using Microsoft.AspNetCore.Identity;

namespace Hybriotheca.Web.Data.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public int? SubscriptionID { get; set; }

    public Subscription? Subscription { get; set; }


    public IEnumerable<Rating>? Ratings { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

}
