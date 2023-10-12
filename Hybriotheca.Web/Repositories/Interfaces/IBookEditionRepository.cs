using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);

    Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync();

    Task<CarouselEditionsViewModel> GetCarouselEditionsAsync();

    Task<List<BookEdition>> CarouselEditionsInfiniteScrollAsync(string category, int lastEditionID);

    Task<SearchViewModel> GetSearchResultsAsync(SearchViewModel viewModel);

    IEnumerable<string> GetCheckBoxCategories();

    IEnumerable<string> GetCheckBoxFormat();

    IEnumerable<string> GetCheckBoxLang();

    IEnumerable<string> GetCheckBoxPubYear();
    Task<bool> IsConstrainedAsync(int id);
    Task UpdateKeepCoverImageAsync(BookEdition bookEdition);
}
