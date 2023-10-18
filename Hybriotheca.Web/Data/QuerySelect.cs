using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models;
using Hybriotheca.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data
{
    public static class QuerySelect
    {
        public static IQueryable<BookStockViewModel> SelectBookStockViewModel(this IQueryable<BookStock> query)
        {
            return query.Select(bookStock => new BookStockViewModel
            {
                Id = bookStock.ID,
                LibraryID = bookStock.LibraryID,
                LibraryName = bookStock.Library.Name,
                BookEditionID = bookStock.BookEditionID,
                BookEditionTitle = bookStock.BookEdition.EditionTitle,
                TotalStock = bookStock.TotalStock,
                AvailableStock = bookStock.AvailableStock,
            });
        }

        public static IQueryable<LoanEmailModel> SelectLoanEmailModel(this IQueryable<Loan> query)
        {
            return query.Select(loan => new LoanEmailModel
            {
                UserFirstName = loan.User.FirstName,
                LoanStatus = loan.Status,
                BookTitle = loan.BookEdition.EditionTitle,
                LibraryName = loan.Library.Name,
                LibraryLocation = loan.Library.Location,
                CreateDate = loan.CreateDate.ToShortDateString(),
                PickupDate = loan.StartDate.ToShortDateString(),
                TermLimit = loan.TermLimitDate.ToShortDateString(),
                ReturnDateOrNull = loan.ReturnDate,
            });
        }

        public static IQueryable<LoanViewModel> SelectLoanViewModel(this IQueryable<Loan> query)
        {
            return query.Select(loan => new LoanViewModel
            {
                Id = loan.ID,
                UserEmail = loan.User.Email ?? "n/a",
                LibraryName = loan.Library.Name,
                BookEditionTitle = loan.BookEdition.EditionTitle,
                StartDate = loan.StartDate.Date,
                TermLimitDate = loan.TermLimitDate,
                Status = loan.Status,
                IsReserved = loan.Status == BookLoanStatus.Reserved,
                IsActive = loan.Status == BookLoanStatus.Active,
                IsOverdue =
                    loan.Status == "Active"
                    && EF.Functions.DateDiffDay(DateTime.UtcNow, loan.ReturnDate) < 0,
            });
        }

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
