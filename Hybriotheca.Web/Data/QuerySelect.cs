using Hybriotheca.Web.Data.Entities;
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
                BookEditionTitle = bookStock.BookEdition.EditionTitle,
                TotalStock = bookStock.TotalStock,
                AvailableStock = bookStock.AvailableStock,
                IsDeletable = bookStock.TotalStock == bookStock.AvailableStock,
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
