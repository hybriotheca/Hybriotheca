using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Models.Entities;

public class BookDetailsViewModel
{
    public BookEdition Book { get; set; }

    public bool hasRating { get; set; }

    public bool HasStock { get; set; }

    public List<Rating> Ratings { get; set; }

    public Rating NewRating { get; set; }

    public int FiveStarPercent 
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 5);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public int FourStarPercent
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 4);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public int ThreeStarPercent
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 3);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public int TwoStarPercent
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 2);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public int OneStarPercent
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 1);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public int ZeroStarPercent
    {
        get
        {
            if (Ratings is not null && Ratings.Any())
            {
                double count = Ratings.Count(r => r.BookRating == 0);
                double percentage = (count / Ratings.Count()) * 100;

                return (int)percentage;
            }
            else
            {
                return 0;
            }
        }
    }

    public List<BookEdition> OtherEditions { get; set; }

    public List<BookEdition> BooksYouMightEnjoy { get; set; }

    public List<BookEdition> OtherBooksBySameAuthor { get; set; }


}
