﻿using Hybriotheca.Web.Data.Entities;
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
            };
        }

        public UserViewModel UserToViewModel(AppUser user)
        {
            return new UserViewModel
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
            };
        }

        public AppUser ViewModelToUser(UserViewModel model)
        {
            return new AppUser
            {
                Id = model.Id ?? Guid.NewGuid().ToString(),
                Role = model.Role,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                SubscriptionID = model.SubscriptionID,
                MainLibraryID = model.MainLibraryID > 0 ? model.MainLibraryID : null,
            };
        }

        public AppUser ViewModelToUser(UserViewModel model, AppUser user)
        {
            user.Id = model.Id;
            user.Role = model.Role;
            user.UserName = model.Email;
            user.Email = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumber = model.PhoneNumber;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.SubscriptionID = model.SubscriptionID;
            user.MainLibraryID = model.MainLibraryID > 0 ? model.MainLibraryID : null;

            return user;
        }
    }
}
