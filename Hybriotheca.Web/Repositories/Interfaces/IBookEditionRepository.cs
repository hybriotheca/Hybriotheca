using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);

    public CarouselEditions GetCarouselEditions();

    public List<BookEdition> CarouselEditionsInfiniteScroll(string category, int lastEditionID);
}
