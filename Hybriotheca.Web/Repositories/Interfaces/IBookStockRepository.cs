using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookStockRepository : IGenericRepository<BookStock>
{
    Task<bool> AnyWhereBookEditionAsync(int bookEditionId);
    Task<bool> ExistsAsync(int libraryId, int bookEditionId);
    Task<int> GetBookStockIdAsync(int libraryId, int bookEditionId);
    Task<BookStock?> GetByLibraryAndBookEditionAsync(int libraryId, int bookEditionId);
    Task<int> GetUsedBookStockAsync(int libraryId, int bookEditionId);
    Task<bool> IsBookAvailableInLibraryAsync(int libraryId, int bookEditionId);
    Task<IEnumerable<BookStockViewModel>> SelectByLibraryAndBookEditionAsync(int libraryId, int bookEditionId);
    Task<IEnumerable<BookStockViewModel>> SelectLastCreatedAsync(int rows);
    Task<BookStockViewModel?> SelectViewModelAsync(int id);
}
