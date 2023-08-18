namespace Hybriotheca.Web.Data.Entities;

public class Reservation : IEntity
{
    public int ID { get; set; }

    public string UserID { get; set; }
    public AppUser User { get; set; }

    public int LibraryID { get; set; }
    public Library Library { get; set; }

    public int BookEditionID { get; set; }
    public BookEdition BookEdition { get; set; }


    public Loan? Loan { get; set; }


    public bool IsActive { get; set; }

    public DateTime Date { get; set; }

}
