using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models;
using Hybriotheca.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<bool> AnyWhereLibraryAndBookEditionAsync(int libraryId, int bookEditionId);
    Task<int> CountBookEditionLoanedFromLibraryAsync(int libraryId, int bookEditionId);
    Task<int> CountUnreturnedWhereUserAsync(string userId);
    IEnumerable<SelectListItem> GetComboBookLoanStatuses();
    Task<LoanEmailModel?> SelectEmailModelAsync(int id);
    Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsync(int rows);
    Task<LoanViewModel?> SelectViewModelAsync(int id);
}
