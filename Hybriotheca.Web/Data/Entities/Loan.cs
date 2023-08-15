using Hybriotheca.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Loan : IEntity
{
    public int ID { get; set; }

    public int EditionID { get; set; }

    public Edition Edition { get; set; } = null!;

    public int LibraryID { get; set; }

    public Library Library { get; set; } = null!;

    public string AppUserID { get; set; } = string.Empty;

    public AppUser User { get; set; } = null!;

    [Display(Name = "Start Date")]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date")]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
    public DateTime EndDate { get; set; }

    [Display(Name = "Past Due Date?")]
    public bool IsPastDueDate { get; set; }

    [Display(Name = "Returned?")]
    public bool IsReturned { get; set; }

    public int? ReservationID { get; set; }

    public Reservation? Reservation { get; set; }

    public Fine? Fine { get; set; }
}
