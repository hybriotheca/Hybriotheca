using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class BookStockRepository : GenericRepository<BookStock>, IBookStockRepository
{
    private readonly DataContext _dataContext;

    public BookStockRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<bool> ExistsAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock.AnyAsync(bookStock =>
            bookStock.LibraryID == libraryId && bookStock.BookEditionID == bookEditionId);
    }

    public async Task<int> GetUsedBookStockAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock =>
                bookStock.LibraryID == libraryId && bookStock.BookEditionID == bookEditionId)
            .Select(bookStock => bookStock.TotalStock - bookStock.AvailableStock)
            .SingleOrDefaultAsync();
    }

    public async Task<BookStock?> GetByLibraryAndBookEditionAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock =>
                bookStock.LibraryID == libraryId && bookStock.BookEditionID == bookEditionId)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<BookStockViewModel>>
        SelectByLibraryAndBookEditionAsListViewModelAsync(int libraryId, int bookEditionId)
    {
        var bookStocks = QueryByLibraryAndBookEdition(libraryId, bookEditionId);

        return await SelectListViewModelAsync(bookStocks);
    }

    public async Task<IEnumerable<BookStockViewModel>> SelectTop25AsListViewModelAsync()
    {
        var bookStocks = _dataContext.BooksInStock
            .OrderByDescending(bookStock => bookStock.ID)
            .Take(25);

        return await SelectListViewModelAsync(bookStocks);
    }

    public async Task<BookStockViewModel?> SelectViewModelAsync(int id)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock => bookStock.ID == id)
            .Select(bookStock => new BookStockViewModel
            {
                Id = bookStock.ID,
                LibraryName = bookStock.Library.Name,
                BookEditionTitle = bookStock.BookEdition.EditionTitle,
                TotalStock = bookStock.TotalStock,
                AvailableStock = bookStock.AvailableStock,
                IsDeletable = bookStock.TotalStock == bookStock.AvailableStock,
            })
            .SingleOrDefaultAsync();
    }

    #region private methods

    private IQueryable<BookStock> QueryByLibraryAndBookEdition(int libraryId, int bookEditionId)
    {
        // Get IQueryable.
        var bookStocks = _dataContext.BooksInStock.AsQueryable();

        // Filter by Library, if id given.
        if (libraryId > 0)
            bookStocks = bookStocks.Where(bookStock => bookStock.LibraryID == libraryId);

        // Filter by Book Edition, if id given.
        if (bookEditionId > 0)
            bookStocks = bookStocks.Where(bookStock => bookStock.BookEditionID == bookEditionId);

        return bookStocks;
    }

    private async Task<IEnumerable<BookStockViewModel>>
        SelectListViewModelAsync(IQueryable<BookStock> bookStocks)
    {
        return await bookStocks.Select(bookStock => new BookStockViewModel
        {
            Id = bookStock.ID,
            LibraryName = bookStock.Library.Name,
            BookEditionTitle = bookStock.BookEdition.EditionTitle,
            TotalStock = bookStock.TotalStock,
            AvailableStock = bookStock.AvailableStock,
            IsDeletable = bookStock.TotalStock == bookStock.AvailableStock,
        }).ToListAsync();
    }

    #endregion
}
