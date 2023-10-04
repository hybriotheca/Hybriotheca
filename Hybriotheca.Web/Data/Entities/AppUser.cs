using Microsoft.AspNetCore.Identity;

namespace Hybriotheca.Web.Data.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string NameAbbr
    {
        get
        {
            var firstLetters = FullName.Split(' ')
                .Where(word => !string.IsNullOrEmpty(word))
                .ToArray();

            string nameAbbr = string.Concat(firstLetters[0][0], firstLetters[firstLetters.Length - 1][0]);

            return nameAbbr;
        }
    }


    public Guid PhotoId { get; set; }

    public string PhotoFullPath => PhotoId == Guid.Empty ?
        "#" :
        "https://hybriotheca.blob.core.windows.net/userphotos/" + PhotoId;


    public int SubscriptionID { get; set; }

    public Subscription Subscription { get; set; }


    public int? MainLibraryID { get; set; }

    public Library? MainLibrary { get; set; }


    public IEnumerable<Rating>? Ratings { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

}
