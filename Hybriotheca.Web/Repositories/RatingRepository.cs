using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class RatingRepository : GenericRepository<Rating>, IRatingRepository
{
    private readonly DataContext _dataContext;

    public RatingRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
