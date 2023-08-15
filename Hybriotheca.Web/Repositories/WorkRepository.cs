using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class WorkRepository : GenericRepository<Work>, IWorkRepository
{
    private readonly DataContext _dataContext;

    public WorkRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
