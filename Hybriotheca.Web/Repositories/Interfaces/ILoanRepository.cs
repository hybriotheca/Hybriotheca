using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<int> CountBookEditionLoanedFromLibraryAsync(int libraryId, int bookEditionId);
    Task<int> CountUnreturnedWhereUserAsync(string userId);
    Task<LoanViewModel?> SelectViewModelAsync(int id);
    Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsListViewModelsAsync(int rows);
}
