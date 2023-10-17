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


    public async Task<bool> AnyWhereBookEditionAsync(int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .AnyAsync(bookStock => bookStock.BookEditionID == bookEditionId);
    }

    public async Task<bool> ExistsAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock.AnyAsync(bookStock =>
            bookStock.LibraryID == libraryId
            && bookStock.BookEditionID == bookEditionId);
    }

    public async Task<int> GetUsedBookStockAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock =>
                bookStock.LibraryID == libraryId
                && bookStock.BookEditionID == bookEditionId)
            .Select(bookStock => bookStock.TotalStock - bookStock.AvailableStock)
            .SingleOrDefaultAsync();
    }

    public async Task<BookStock?> GetByLibraryAndBookEditionAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock =>
                bookStock.LibraryID == libraryId
                && bookStock.BookEditionID == bookEditionId)
            .SingleOrDefaultAsync();
    }

    public async Task<bool> IsBookAvailableInLibraryAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock =>
                bookStock.LibraryID == libraryId
                && bookStock.BookEditionID == bookEditionId)
            .Select(bookStock => bookStock.AvailableStock > 0)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<BookStockViewModel>>
        SelectByLibraryAndBookEditionAsync(int libraryId, int bookEditionId)
    {
        // Get IQueryable.
        var bookStocks = _dataContext.BooksInStock.AsQueryable();

        // Filter by Library, if id given.
        if (libraryId > 0)
            bookStocks = bookStocks.Where(bookStock => bookStock.LibraryID == libraryId);

        // Filter by Book Edition, if id given.
        if (bookEditionId > 0)
            bookStocks = bookStocks.Where(bookStock => bookStock.BookEditionID == bookEditionId);

        return await bookStocks
            .SelectBookStockViewModel()
            .ToListAsync();
    }

    public async Task<IEnumerable<BookStockViewModel>> SelectLastCreatedAsync(int rows)
    {
        return await _dataContext.BooksInStock
            .OrderByDescending(bookStock => bookStock.ID)
            .Take(rows)
            .SelectBookStockViewModel()
            .ToListAsync();
    }

    public async Task<BookStockViewModel?> SelectViewModelAsync(int id)
    {
        return await _dataContext.BooksInStock
            .Where(bookStock => bookStock.ID == id)
            .SelectBookStockViewModel()
            .SingleOrDefaultAsync();
    }
}
