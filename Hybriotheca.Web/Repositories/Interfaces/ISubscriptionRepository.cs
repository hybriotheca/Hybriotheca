using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<Subscription> GetByNameAsync(string name);

    Task<Subscription?> GetByIdWithUsers(int id);
    Task<IEnumerable<SelectListItem>> GetComboSubscriptionsAsync();
    Task<string?> GetSubscriptionNameAsync(int subscriptionId);
    Task<int> GetDefaultSubscriptionIdForNewUserAsync();
    Task<bool> IsConstrainedAsync(int id);
}
