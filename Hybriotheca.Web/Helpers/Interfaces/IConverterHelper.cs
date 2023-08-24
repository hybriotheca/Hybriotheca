using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IConverterHelper
    {
        BookEditionViewModel BookEditionToViewModel(BookEdition bookEdition);
        BookEdition ViewModelToBookEdition(BookEditionViewModel model);
    }
}