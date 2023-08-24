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
    }
}
