using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class BookEdition : IEntity
{
    public int ID { get; set; }

    [Display(Name = "Base Book")]
    public int BookID { get; set; }
    public Book Book { get; set; }

    [Display(Name = "Category")]
    public int CategoryID { get; set; }
    public Category Category { get; set; }


    public string? ISBN { get; set; }

    [Display(Name = "Title")]
    public string EditionTitle { get; set; }

    public string Synopsis { get; set; }

    public string Publisher { get; set; }

    [Display(Name = "Publish Date")]
    public DateTime PublishDate { get; set; }

    public string Language { get; set; }

    [Display(Name = "Translation Author")]
    public string? TranslationAuthor { get; set; }

    public string? Awards { get; set; }

    [Display(Name = "Book Format")]
    public string BookFormat { get; set; }

    [Display(Name = "Nº Pages")]
    public int NrPages { get; set; }

    [Display(Name = "Available Online")]
    public bool IsAvailableOnline { get; set; }

    public Guid? EbookID { get; set; }

    public Guid CoverImageID { get; set; }

    [Display(Name = "Cover")]
    public string CoverImageFullPath => CoverImageID == Guid.Empty ?
        "" : $"https://hybriotheca.blob.core.windows.net/bookcovers/{CoverImageID}";


    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Rating>? Ratings { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

}