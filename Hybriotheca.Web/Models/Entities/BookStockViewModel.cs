namespace Hybriotheca.Web.Models.Entities
{
    public class BookStockViewModel
    {
        public int Id { get; set; }


        public int LibraryID { get; set; }

        public string LibraryName { get; set; }


        public int BookEditionID { get; set; }

        public string BookEditionTitle { get; set; }


        public int TotalStock { get; set; }

        public int AvailableStock { get; set; }
    }
}
