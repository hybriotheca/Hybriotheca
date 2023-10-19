using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<IEnumerable<SelectListItem>> GetComboSubscriptionsAsync();
    Task<int> GetDefaultSubscriptionIdForNewUserAsync();
    Task<int> GetPremiumSubscriptionIdAsync();
    Task<bool> IsConstrainedAsync(int id);
}
