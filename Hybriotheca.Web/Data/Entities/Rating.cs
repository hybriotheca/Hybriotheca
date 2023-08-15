using Hybriotheca.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Rating : IEntity
{
    public int ID { get; set; }

    public int EditionID { get; set; }

    public Edition Edition { get; set; } = null!;

    public string AppUserID { get; set; } = string.Empty;

    public AppUser User { get; set; } = null!;

    [Display(Name = "Rating")]
    [Range(0, 10, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int BookRating { get; set; }

    [Display(Name = "Title")]
    [MaxLength(25, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string RatingTitle { get; set; } = string.Empty;

    [Display(Name = "Write your review")]
    [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string RatingBody { get; set; } = string.Empty;

}