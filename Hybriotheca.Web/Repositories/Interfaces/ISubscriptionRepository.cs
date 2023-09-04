using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<Subscription> GetByNameAsync(string name);

    Task<Subscription> GetByIdWithUsers(int id);
    Task<IEnumerable<SelectListItem>> GetComboSubscriptions();
    Task<string?> GetSubscriptionNameAsync(int subscriptionId);
}
