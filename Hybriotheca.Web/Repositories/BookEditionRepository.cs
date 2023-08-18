using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class BookEditionRepository : GenericRepository<BookEdition>, IBookEditionRepository
{
    private readonly DataContext _dataContext;

    public BookEditionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
