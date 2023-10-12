using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    public List<BookEdition> CarouselEditionsInfiniteScroll(string category, int lastEditionID);
    public CarouselEditions GetCarouselEditions();
    Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync();
    Task<bool> IsConstrainedAsync(int id);
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);
}
