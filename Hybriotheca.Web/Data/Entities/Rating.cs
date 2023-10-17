using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

[Index(nameof(UserID), nameof(BookEditionID), IsUnique = true, Name = "IX_AppUser_BookEdition")]
public class Rating : IEntity
{
    public int ID { get; set; }

    public string UserID { get; set; }
    public AppUser User { get; set; }

    public int BookEditionID { get; set; }
    public BookEdition BookEdition { get; set; }

    [Display(Name = "My rating")]
    [Range(1, 5, ErrorMessage = "Please choose an acceptable value")]
    public int BookRating { get; set; }

    [Display(Name = "Title")]
    public string? RatingTitle { get; set; }

    [Display(Name = "What did you think?")]
    public string? RatingBody { get; set; }

    public DateTime RatingDate { get; set; } = DateTime.UtcNow;

}