using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Data
{
    public static class QuerySelect
    {
        public static IQueryable<UserViewModel> SelectUserViewModel(this IQueryable<AppUser> query)
        {
            return query.Select(user => new UserViewModel
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HasPhoto = user.PhotoId != Guid.Empty,
                PhotoFullPath = user.PhotoFullPath,
                SubscriptionID = user.SubscriptionID,
                MainLibraryID = user.MainLibraryID ?? 0,
            });
        }
    }
}
