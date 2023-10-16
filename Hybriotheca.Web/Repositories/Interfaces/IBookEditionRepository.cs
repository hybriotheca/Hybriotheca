using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookEditionRepository : IGenericRepository<BookEdition>
{
    Task<List<BookEdition>> CarouselEditionsInfiniteScrollAsync(string category, int lastEditionID);

    Task<CarouselEditionsViewModel> GetCarouselEditionsAsync();

    IEnumerable<string> GetCheckBoxCategories();

    IEnumerable<string> GetCheckBoxFormat();

    IEnumerable<string> GetCheckBoxLang();

    IEnumerable<string> GetCheckBoxLibraries();

    IEnumerable<string> GetCheckBoxPubYear();

    Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync();

    Task<SearchViewModel> GetSearchResultsAsync(SearchViewModel viewModel);

    Task<bool> IsConstrainedAsync(int id);

    (Guid CoverID, Guid ePubID) GetCoverIDAndEpubID(int ID);

    List<SelectListItem> GetComboBookFormats();

    List<SelectListItem> GetComboLanguages();
}
