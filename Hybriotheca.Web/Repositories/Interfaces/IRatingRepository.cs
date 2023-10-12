using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IRatingRepository : IGenericRepository<Rating>
{
    Task<List<Rating>> GetRatingsByBookID(int id);
    Task<Rating> GetByIDWithAll(int id);
    Task<bool> AnyWhereBookEditionAsync(int bookEditionId);
}
