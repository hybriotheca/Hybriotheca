using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class FineRepository : GenericRepository<Fine>, IFineRepository
{
    private readonly DataContext _dataContext;

    public FineRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
