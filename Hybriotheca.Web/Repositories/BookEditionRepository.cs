using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class BookEditionRepository : GenericRepository<BookEdition>, IBookEditionRepository
{
    private readonly DataContext _dataContext;

    public BookEditionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync()
    {
        return await _dataContext.BookEditions
            .Select(bookEdition => new SelectListItem
            {
                Text = bookEdition.EditionTitle,
                Value = bookEdition.ID.ToString(),
            }).ToListAsync();
    }

    public async Task<bool> IsConstrainedAsync(int id)
    {
        return await _dataContext.BookEditions
            .Where(bookEdition => bookEdition.ID == id)
            .AnyAsync(bookEdition =>
                bookEdition.BooksInStock.Any()
                || bookEdition.Loans.Any()
                || bookEdition.Ratings.Any()
                || bookEdition.Reservations.Any());
    }

    public async Task UpdateKeepCoverImageAsync(BookEdition bookEdition)
    {
        _dataContext.Set<BookEdition>().Update(bookEdition);
        _dataContext.Entry(bookEdition).Property(b => b.CoverImageID).IsModified = false;
        await _dataContext.SaveChangesAsync();
    }

    public CarouselEditions GetCarouselEditions()
    {
        var adventure = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Adventure").Take(4).ToList();

        var fantasy = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Fantasy").Take(4).ToList();

        var romance = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Romance").Take(4).ToList();

        var science = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Science").Take(4).ToList();

        var scifi = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Sci-fi").Take(4).ToList();

        CarouselEditions CarouselEditions = new CarouselEditions()
        {
            Adventure = adventure,
            Fantasy = fantasy,
            Romance = romance,
            Science = science,
            Scifi = scifi
        };

        return CarouselEditions;
    }

    public List<BookEdition> CarouselEditionsInfiniteScroll(string category, int lastEditionID)
    {
        if (string.IsNullOrEmpty(category))
        {
            return new List<BookEdition>();
        }

        var list = _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == category).Where(s => s.ID > lastEditionID).Take(4).ToList();

        return list;
    }
}
