using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    IEnumerable<SelectListItem> GetComboCategories();
    Task<bool> IsConstrainedAsync(int id);
}
