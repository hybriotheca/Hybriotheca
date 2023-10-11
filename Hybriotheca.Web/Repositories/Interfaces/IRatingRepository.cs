using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IRatingRepository : IGenericRepository<Rating>
{
    Task<bool> AnyWhereBookEditionAsync(int bookEditionId);
}
