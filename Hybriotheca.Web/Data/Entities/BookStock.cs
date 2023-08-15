using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

//[PrimaryKey(nameof(BookID), nameof(LibraryID))]
[Index(nameof(EditionID), nameof(LibraryID), IsUnique = true, Name = "IX_EditionID_LibraryID")]
public class BookStock : IEntity
{
    public int ID { get; set; }

    public int EditionID { get; set; }

    public Edition Edition { get; set; } = null!;

    public int LibraryID { get; set; }

    public Library Library { get; set; } = null!;

    [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
    public int QtyInStock { get; set; }
    
}