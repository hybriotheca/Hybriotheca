using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookStockRepository : IGenericRepository<BookStock>
{
    Task<bool> AnyWhereBookEditionAsync(int bookEditionId);
    Task<bool> ExistsAsync(int libraryId, int bookEditionId);
    Task<BookStock?> GetByLibraryAndBookEditionAsync(int libraryId, int bookEditionId);
    Task<int> GetUsedBookStockAsync(int libraryId, int bookEditionId);
    Task<bool> IsBookAvailableInLibraryAsync(int libraryId, int bookEditionId);
    Task<IEnumerable<BookStockViewModel>> SelectByLibraryAndBookEditionAsListViewModelAsync(int libraryId, int bookEditionId);
    Task<IEnumerable<BookStockViewModel>> SelectTop25AsListViewModelAsync();
    Task<BookStockViewModel?> SelectViewModelAsync(int id);
}
