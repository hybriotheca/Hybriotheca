using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);

    public CarouselEditions GetCarouselEditions();

    public List<BookEdition> CarouselEditionsInfiniteScroll(string category, int lastEditionID);
    Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync();
    Task<bool> IsConstrainedAsync(int id);
}
