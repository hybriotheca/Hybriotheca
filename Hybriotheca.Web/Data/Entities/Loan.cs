namespace Hybriotheca.Web.Data.Entities;

public class Loan : IEntity
{
    public int ID { get; set; }

    public string UserID { get; set; }
    public AppUser User { get; set; }

    public int LibraryID { get; set; }
    public Library Library { get; set; }

    public int BookEditionID { get; set; }
    public BookEdition BookEdition { get; set; }

    public int? ReservationID { get; set; }
    public Reservation? Reservation { get; set; }


    public Fine? Fine { get; set; }


    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsReturned { get; set; }

}
