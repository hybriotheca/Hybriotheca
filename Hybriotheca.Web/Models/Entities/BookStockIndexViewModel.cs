namespace Hybriotheca.Web.Models.Entities
{
    public class BookStockIndexViewModel
    {
        public IEnumerable<BookStockViewModel> BookStocks { get; set; }

        public BookStockViewModel SearchModel { get; set; }
    }
}
