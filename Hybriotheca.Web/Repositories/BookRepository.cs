using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    private readonly DataContext _dataContext;

    public BookRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public IEnumerable<SelectListItem> GetComboBooks()
    {
        return _dataContext.Books.Select(c => new SelectListItem
        {
            Text = c.OriginalTitle,
            Value = c.ID.ToString()
        });
    }
}
