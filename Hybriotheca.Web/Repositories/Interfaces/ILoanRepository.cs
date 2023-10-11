using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<int> CountBookEditionLoanedFromLibraryAsync(int libraryId, int bookEditionId);
    Task<LoanViewModel?> SelectViewModelAsync(int id);
    Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsListViewModelsAsync(int rows);
    Task<bool> AnyWhereBookEditionAsync(int bookEditionId);
}
