using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<IEnumerable<SelectListItem>> GetComboBooksAsync();
    Task<bool> IsConstrainedAsync(int id);
}
