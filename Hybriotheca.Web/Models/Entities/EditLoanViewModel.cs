using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class EditLoanViewModel
    {
        public int Id { get; set; }

        public int LibraryId { get; set; }

        public string UserId { get; set; }

        public int BookEditionId { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsReturned { get; set; }
    }
}
