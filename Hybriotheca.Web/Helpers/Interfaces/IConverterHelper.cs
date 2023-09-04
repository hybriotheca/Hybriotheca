using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IConverterHelper
    {
        BookEditionViewModel BookEditionToViewModel(BookEdition bookEdition);
        UserViewModel UserToViewModel(AppUser user);
        BookEdition ViewModelToBookEdition(BookEditionViewModel model);
        AppUser ViewModelToUser(UserViewModel model);
        AppUser ViewModelToUser(UserViewModel model, AppUser user);
    }
}