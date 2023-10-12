using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Library : IEntity
{
    public int ID { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Location { get; set; }

    [Required]
    public string Contact { get; set; }


    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

    public IEnumerable<AppUser>? Users { get; set; }
}
