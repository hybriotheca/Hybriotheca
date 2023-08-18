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
    [RegularExpression(@"^(9\d|2\d)[\d]{7}$", ErrorMessage = "Please insert a valid Portuguese phone number")]
    public string Contact { get; set; } = string.Empty;


    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

}
