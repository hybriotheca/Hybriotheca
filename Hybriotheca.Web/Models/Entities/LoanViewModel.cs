using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class LoanViewModel
    {
        public int Id { get; set; }

        public string LibraryName { get; set; }

        public string UserEmail { get; set; }

        public string BookEditionTitle { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsReturned { get; set; }

        public bool IsOverdue { get; set; }
    }
}
