using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class BookEditionRepository : GenericRepository<BookEdition>, IBookEditionRepository
{
    private readonly DataContext _dataContext;

    public BookEditionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task UpdateKeepCoverImageAsync(BookEdition bookEdition)
    {
        _dataContext.Set<BookEdition>().Update(bookEdition);
        _dataContext.Entry(bookEdition).Property(b => b.CoverImageID).IsModified = false;
        await _dataContext.SaveChangesAsync();
    }
}
