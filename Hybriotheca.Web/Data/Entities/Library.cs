using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Library : IEntity
{
    public int ID { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string Location { get; set; } = string.Empty;

    [MaxLength(9, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    [RegularExpression(@"^(9\d|2\d)[\d]{7}$", ErrorMessage = "Please insert a valid Portuguese phone number")]
    public string Contact { get; set; } = string.Empty;

    public IEnumerable<BookStock> BooksInStock { get; set; } = null!;

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }
}
