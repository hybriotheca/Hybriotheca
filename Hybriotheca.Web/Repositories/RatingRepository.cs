using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class RatingRepository : GenericRepository<Rating>, IRatingRepository
{
    private readonly DataContext _dataContext;

    public RatingRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<List<Rating>> GetRatingsByBookID(int id)
    {
        return await _dataContext.Ratings.Include(s => s.BookEdition).AsSplitQuery().Include(e => e.User).AsSplitQuery().Where(z => z.BookEditionID == id).AsNoTracking().ToListAsync();
    }

    public async Task<List<Rating>> GetRatingsByBookIDWithOtherUserRatings(int id)
    {
        return await _dataContext.Ratings.Include(s => s.BookEdition).AsSplitQuery().Include(e => e.User).ThenInclude(q => q.Ratings).Where(z => z.BookEditionID == id).ToListAsync();
    }

    public async Task<Rating> GetByIDWithAll(int id)
    {
        return await _dataContext.Ratings.Include(s => s.BookEdition).AsSplitQuery().Include(e => e.User).AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(z => z.ID == id);
    }



    public async Task<bool> AnyWhereBookEditionAsync(int bookEditionId)
    {
        return await _dataContext.Ratings.AnyAsync(rating => rating.BookEditionID == bookEditionId);
    }
}
