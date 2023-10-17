using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                && loan.Status != BookLoanStatus.Returned);
    }

    public async Task<int> CountUnreturnedWhereUserAsync(string userId)
    {
        return await _dataContext.Loans
            .CountAsync(loan => loan.UserID == userId && loan.Status != BookLoanStatus.Returned);
    }

    public IEnumerable<SelectListItem> GetComboBookLoanStatuses()
    {
        return BookLoanStatus.All.Select(status => new SelectListItem
        {
            Text = status,
            Value = status
        });
    }

    public async Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsListViewModelsAsync(int rows)
    {
        return await _dataContext.Loans
            .SelectLoanViewModel()
            .OrderByDescending(loan => loan.Id)
            .Take(rows)
            .ToListAsync();
    }

    public async Task<LoanViewModel?> SelectViewModelAsync(int id)
    {
        return await _dataContext.Loans
            .Where(loan => loan.ID == id)
            .SelectLoanViewModel()
            .SingleOrDefaultAsync();
    }
}
