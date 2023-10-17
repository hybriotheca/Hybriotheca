﻿using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Repositories.Interfaces;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<int> CountBookEditionLoanedFromLibraryAsync(int libraryId, int bookEditionId);
    Task<int> CountUnreturnedWhereUserAsync(string userId);
    IEnumerable<SelectListItem> GetComboBookLoanStatuses();
    Task<LoanViewModel?> SelectViewModelAsync(int id);
    Task<IEnumerable<LoanViewModel>> SelectLastCreatedAsListViewModelsAsync(int rows);
}
