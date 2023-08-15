using Hybriotheca.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Reservation : IEntity
{
    public int ID { get; set; }

    public int EditionID { get; set; }

    public Edition Edition { get; set; } = null!;

    public string AppUserID { get; set; } = string.Empty;

    public AppUser User { get; set; } = null!;

    public int LibraryID { get; set; }

    public Library Library { get; set; } = null!;

    [Display(Name = "Is Reservation active?")]
    public bool IsActive { get; set; }

    [Display(Name = "Reservation Date")]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
    public DateTime ReservationDate { get; set; }

    public Loan? Loan { get; set; }

}