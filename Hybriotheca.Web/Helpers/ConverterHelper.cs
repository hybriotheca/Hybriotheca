using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public BookEditionViewModel BookEditionToViewModel(BookEdition bookEdition)
        {
            return new BookEditionViewModel
            {
                ID = bookEdition.ID,
                BookID = bookEdition.BookID,
                CategoryID = bookEdition.CategoryID,
                ISBN = bookEdition.ISBN,
                Title = bookEdition.EditionTitle,
                Synopsis = bookEdition.Synopsis,
                Publisher = bookEdition.Publisher,
                PublishDate = bookEdition.PublishDate,
                Language = bookEdition.Language,
                TranslationAuthor = bookEdition.TranslationAuthor,
                Awards = bookEdition.Awards,
                BookFormat = bookEdition.BookFormat,
                NrPages = bookEdition.NrPages,
                CoverImageFullPath = bookEdition.CoverImageFullPath,
                EbookID = bookEdition.EbookID,
                IsAvailableOnline = bookEdition.IsAvailableOnline,
            };
        }

        public UserViewModel UserToViewModel(AppUser user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HasPhoto = user.PhotoId != Guid.Empty,
                PhotoFullPath = user.PhotoFullPath,
                SubscriptionID = user.SubscriptionID
            };
        }


        public BookEdition ViewModelToBookEdition(BookEditionViewModel model)
        {
            return new BookEdition
            {
                ID = model.ID,
                BookID = model.BookID,
                CategoryID = model.CategoryID,
                ISBN = model.ISBN,
                EditionTitle = model.Title,
                Synopsis = model.Synopsis,
                Publisher = model.Publisher,
                PublishDate = model.PublishDate,
                Language = model.Language,
                TranslationAuthor = model.TranslationAuthor,
                Awards = model.Awards,
                BookFormat = model.BookFormat,
                NrPages = model.NrPages,
                
                EbookID = model.EbookID,
                IsAvailableOnline = model.IsAvailableOnline,
            };
        }

        public AppUser ViewModelToUser(UserViewModel model)
        {
            return new AppUser
            {
                Id = model.Id ?? Guid.NewGuid().ToString(),
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                SubscriptionID = model.SubscriptionID,
            };
        }

        public AppUser ViewModelToUser(UserViewModel model, AppUser user)
        {
            user.Id = model.Id;
            user.UserName = model.Email;
            user.Email = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumber = model.PhoneNumber;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.SubscriptionID = model.SubscriptionID;

            return user;
        }
    }
}
