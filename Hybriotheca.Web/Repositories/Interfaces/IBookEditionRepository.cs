using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);
}
