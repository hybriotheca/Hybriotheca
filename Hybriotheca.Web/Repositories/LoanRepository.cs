using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    private readonly DataContext _dataContext;

    public LoanRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<int> CountBookEditionLoanedFromLibraryAsync(int libraryId, int bookEditionId)
    {
        return await _dataContext.Loans
            .CountAsync(loan =>
                loan.LibraryID == libraryId
                && loan.BookEditionID == bookEditionId
                && !loan.IsReturned);
    }

    public async Task<int> CountUnreturnedWhereUserAsync(string userId)
    {
        return await _dataContext.Loans.CountAsync(loan => loan.UserID == userId && !loan.IsReturned);
    }

    public async Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsListViewModelsAsync(int rows)
    {
        return await _dataContext.Loans
            .Select(loan => new LoanViewModel
            {
                Id = loan.ID,
                LibraryName = loan.Library.Name,
                UserEmail = loan.User.Email ?? "n/a",
                BookEditionTitle = loan.BookEdition.EditionTitle,
                StartDate = loan.StartDate.Date,
                EndDate = loan.EndDate.Date,
                IsReturned = loan.IsReturned,
                IsOverdue = EF.Functions.DateDiffDay(DateTime.UtcNow, loan.EndDate) < 0,
            })
            .OrderByDescending(loan => loan.Id)
            .Take(rows)
            .ToListAsync();
    }

    public async Task<LoanViewModel?> SelectViewModelAsync(int id)
    {
        return await _dataContext.Loans
            .Where(loan => loan.ID == id)
            .Select(loan => new LoanViewModel
            {
                Id = loan.ID,
                LibraryName = loan.Library.Name,
                UserEmail = loan.User.Email ?? "n/a",
                BookEditionTitle = loan.BookEdition.EditionTitle,
                StartDate = loan.StartDate.Date,
                EndDate = loan.EndDate.Date,
                IsReturned = loan.IsReturned,
                IsOverdue = EF.Functions.DateDiffDay(DateTime.UtcNow, loan.EndDate) < 0,
            }).SingleOrDefaultAsync();
    }
}
