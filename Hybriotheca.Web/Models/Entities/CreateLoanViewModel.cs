using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class CreateLoanViewModel
    {
        [Display(Name = "Library")]
        [Range(1, int.MaxValue, ErrorMessage = "Library field is required")]
        public int LibraryId { get; set; }

        [Display(Name = "Book Edition")]
        public int BookEditionId { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; }


        [Display(Name = "Checking out later?")]
        public bool WillCheckOutLater { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Check out date")]
        public DateTime StartDate { get; set; }
    }
}
