using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class LibraryRepository : GenericRepository<Library>, ILibraryRepository
{
    private readonly DataContext _dataContext;

    public LibraryRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<IEnumerable<SelectListItem>> GetComboLibrariesAsync()
    {
        return await _dataContext.Libraries
            .Select(library => new SelectListItem
            {
                Text = library.Name,
                Value = library.ID.ToString(),
            }).ToListAsync();
    }

    public async Task<bool> IsConstrainedAsync(int id)
    {
        return await _dataContext.Libraries
            .Where(library => library.ID == id)
            .AnyAsync(library =>
                library.BooksInStock.Any()
                || library.Loans.Any()
                || library.Reservations.Any()
                || library.Users.Any());
    }
}
