using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class LibraryRepository : GenericRepository<Library>, ILibraryRepository
{
    private readonly DataContext _dataContext;

    public LibraryRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
