using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    private readonly DataContext _dataContext;

    public SubscriptionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<IEnumerable<SelectListItem>> GetComboSubscriptionsAsync()
    {
        return await _dataContext.Subscriptions.Select(s => new SelectListItem
        {
            Text = s.Name,
            Value = s.ID.ToString(),
        }).ToListAsync();
    }

    public async Task<int> GetDefaultSubscriptionIdForNewUserAsync()
    {
        return await _dataContext.Subscriptions
            .Where(s => s.Name == "Standard")
            .Select(s => s.ID)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsConstrainedAsync(int id)
    {
        return await _dataContext.Subscriptions
            .Where(subscription => subscription.ID == id)
            .AnyAsync(subscription => subscription.Users.Any());
    }
}
