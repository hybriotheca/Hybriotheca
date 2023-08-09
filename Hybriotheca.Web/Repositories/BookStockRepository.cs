using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class BookStockRepository : GenericRepository<BookStock>, IBookStockRepository
{
    private readonly DataContext _dataContext;

    public BookStockRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
