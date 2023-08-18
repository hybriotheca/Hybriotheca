﻿namespace Hybriotheca.Web.Data.Entities;

public class BookEdition : IEntity
{
    public int ID { get; set; }

    public int BookID { get; set; }
    public Book Book { get; set; }

    public int CategoryID { get; set; }
    public Category Category { get; set; }


    public string? ISBN { get; set; }

    public string EditionTitle { get; set; }

    public string Synopsis { get; set; }

    public string Publisher { get; set; }

    public DateTime PublishDate { get; set; }

    public string Language { get; set; }

    public string? TranslationAuthor { get; set; }

    public string? Awards { get; set; }

    public string BookFormat { get; set; }

    public int NrPages { get; set; }

    public bool IsAvailableOnline { get; set; }

    public Guid? EbookID { get; set; }

    public Guid CoverImageID { get; set; }

    public string CoverImageFullPath => CoverImageID == Guid.Empty ?
        "" : $"https://hybriotheca.blob.core.windows.net/bookcovers/{CoverImageID}";


    public IEnumerable<BookStock>? BooksInStock { get; set; }

    public IEnumerable<Loan>? Loans { get; set; }

    public IEnumerable<Rating>? Ratings { get; set; }

    public IEnumerable<Reservation>? Reservations { get; set; }

}