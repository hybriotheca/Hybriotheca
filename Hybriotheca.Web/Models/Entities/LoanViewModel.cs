using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class LoanViewModel
    {
        public int Id { get; set; }


        [Display(Name = "User")]
        public string UserEmail { get; set; }

        [Display(Name = "Library")]
        public string LibraryName { get; set; }

        [Display(Name = "Book")]
        public string BookEditionTitle { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Start")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Term limit")]
        public DateTime TermLimitDate { get; set; }


        public string Status { get; set; }


        public bool IsReserved { get; set; }

        public bool IsActive { get; set; }

        public bool IsOverdue { get; set; }
    }
}
