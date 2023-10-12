using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ILibraryRepository : IGenericRepository<Library>
{
    Task<IEnumerable<SelectListItem>> GetComboLibrariesAsync();
    Task<bool> IsConstrainedAsync(int id);
}
