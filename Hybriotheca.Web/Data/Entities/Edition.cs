using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Edition : IEntity
{
    public int ID { get; set; }

    public int CategoryID { get; set; }

    public Category Category { get; set; } = null!;

    public int WorkID { get; set; }

    public Work Work { get; set; } = null!;

    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Rating>? Ratings { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

    [MaxLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string EditionTitle { get; set; } = string.Empty;

    [MaxLength(13, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string? ISBN { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string? TranslationAuthor { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string BookFormat { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string Publisher { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string Language { get; set; } = string.Empty;

    [Display(Name = "Publish Date")]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
    public DateTime PublishDate { get; set; }

    [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string? Awards { get; set; }

    public Guid? EbookID { get; set; }

    public Guid CoverImageID { get; set; }

    [MaxLength(2500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string? Sinopse { get; set; }

    [Display(Name = "Number Of Pages")]
    [Range(1,int.MaxValue,ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int NrPages { get; set; }

    [Display(Name = "Available Online?")]
    public bool IsAvailableOnline { get; set; }

}