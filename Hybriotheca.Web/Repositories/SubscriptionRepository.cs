using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    private readonly DataContext _dataContext;

    public SubscriptionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Subscription> GetByNameAsync(string name)
    {
        return await _dataContext.Subscriptions.AsNoTracking().FirstAsync(x => x.Name == name);

    }

    public async Task<Subscription> GetByIdWithUsers(int id)
    {
        return await _dataContext.Subscriptions.Where(s => s.ID == id).Include(x => x.Users).FirstOrDefaultAsync();
    }

}
