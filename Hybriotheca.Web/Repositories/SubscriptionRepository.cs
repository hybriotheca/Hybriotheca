using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    private readonly DataContext _dataContext;

    public SubscriptionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
