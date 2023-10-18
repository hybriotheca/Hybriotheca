namespace Hybriotheca.Web.Models
{
    public class LoanEmailModel
    {
        public string UserFirstName { get; set; }

        public string LoanStatus { get; set; }

        public string BookTitle { get; set; }

        public string LibraryName { get; set; }

        public string LibraryLocation { get; set; }

        public string CreateDate { get; set; }

        public string PickupDate { get; set; }

        public string TermLimit { get; set; }

        public DateTime? ReturnDateOrNull { get; set; }

        public string ReturnDate =>
            ReturnDateOrNull == null ? "n/a"
            : ReturnDateOrNull.Value.ToShortDateString();
    }
}
