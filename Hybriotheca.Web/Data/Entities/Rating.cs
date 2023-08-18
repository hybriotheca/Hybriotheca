namespace Hybriotheca.Web.Data.Entities;

public class Rating : IEntity
{
    public int ID { get; set; }

    public string UserID { get; set; }
    public AppUser User { get; set; }

    public int BookEditionID { get; set; }
    public BookEdition BookEdition { get; set; }


    public int BookRating { get; set; }

    public string RatingTitle { get; set; }

    public string RatingBody { get; set; }

}