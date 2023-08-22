using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<Subscription> GetByNameAsync(string name);

    Task<Subscription> GetByIdWithUsers(int id);
}
