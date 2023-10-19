namespace Hybriotheca.Web.Data.Entities;

public class Library : IEntity
{
    public int ID { get; set; }

    public string Name { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public string Location => $"{City}, {Country}";

    public string PhoneNumber { get; set; }

    public string Email { get; set; }


    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

    public IEnumerable<AppUser>? Users { get; set; }
}
