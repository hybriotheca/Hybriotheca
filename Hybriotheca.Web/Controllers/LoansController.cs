using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class LoansController : Controller
    {
        private readonly IUserHelper _userHelper;

        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IBookStockRepository _bookStockRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILoanRepository _loanRepository;

        public LoansController(
            IUserHelper userHelper,
            IBookEditionRepository bookEditionRepository,
            IBookStockRepository bookStockRepository,
            ILibraryRepository libraryRepository,
            ILoanRepository loanRepository)
        {
            _userHelper = userHelper;
            _bookEditionRepository = bookEditionRepository;
            _bookStockRepository = bookStockRepository;
            _libraryRepository = libraryRepository;
            _loanRepository = loanRepository;
        }


        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _loanRepository.SelectLastCreatedAsListViewModelsAsync(25);

            return View(loans);
            //return _context.Loans != null ?
            //            View(await _context.Loans.ToListAsync()) :
            //            Problem("Entity set 'DataContext.Loans'  is null.");
        }


        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _loanRepository.SelectViewModelAsync(id.Value);
            if (model == null) return NotFound();

            // Success.
            return View(model);
        }


        // GET: Loans/Create
        public async Task<IActionResult> Create(int? bookEditionId)
        {
            var model = new CreateLoanViewModel
            {
                BookEditionId = bookEditionId ?? 0,
                LibraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName()) ?? 0,
            };

            return await ViewCreateAsync(model);
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null) return NotFound();

                model.LibraryId = libraryId.Value;
            }

            if (ModelState.IsValid)
            {
                var bookStock = await _bookStockRepository
                    .GetByLibraryAndBookEditionAsync(model.LibraryId, model.BookEditionId);

                if (bookStock == null) return NotFound();

                var dateToday = DateTime.UtcNow.Date;
                var loan = new Loan
                {
                    UserID = model.UserId,
                    LibraryID = model.LibraryId,
                    BookEditionID = model.BookEditionId,
                    ReservationID = null,
                    StartDate = dateToday,
                    EndDate = dateToday.AddDays(14),
                    IsReturned = false,
                };

                try
                {
                    // BookStock is updated on SaveChangesAsync() inside CreateAsync().
                    bookStock.AvailableStock--;
                    await _loanRepository.CreateAsync(loan);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is SqlException innerEx)
                    {
                        string constraintName =
                            $"CK_{nameof(bookStock.AvailableStock)}_GreaterOrEqualZero";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            AddModelError(
                                "There isn't available Book Stock at this Library for this Loan.");
                            return await ViewCreateAsync(model);
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(Loan)}.");
            return await ViewCreateAsync(model);
        }


        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _loanRepository.GetByIdAsync(id.Value);
            if (loan == null) return NotFound();

            return await ViewEditAsync(loan);
        }

        // POST: Loans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Loan loan)
        {
            // Remove Loan's navigation properties from ModelState validation.
            ModelState.Remove(nameof(loan.BookEdition));
            ModelState.Remove(nameof(loan.Library));
            ModelState.Remove(nameof(loan.User));

            if (ModelState.IsValid)
            {
                var current = await _loanRepository.GetByIdAsync(loan.ID);
                if (current == null) return NotFound();

                if (!(current.IsReturned && loan.IsReturned))
                {
                    // Update AvailableStocks.

                    // If Loan was previously not returned, increment its BookStock AvailableStock.
                    if (!current.IsReturned)
                    {
                        var bookStockCurrent = await _bookStockRepository
                            .GetByLibraryAndBookEditionAsync(current.LibraryID, current.BookEditionID);

                        if (bookStockCurrent == null) return NotFound();

                        bookStockCurrent.AvailableStock++;
                    }

                    // If the edited Loan is not returned, decrement its BookStock AvailableStock.
                    if (!loan.IsReturned)
                    {
                        var bookStockEdited = await _bookStockRepository
                            .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

                        if (bookStockEdited == null)
                        {
                            AddModelError("This book is not available in this Library.");
                            return await ViewEditAsync(loan);
                        }

                        bookStockEdited.AvailableStock--;
                    }
                }

                try
                {
                    // Update Loan and save BookStocks' changes.
                    await _loanRepository.UpdateAsync(loan);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _loanRepository.ExistsAsync(loan.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is SqlException innerEx)
                    {
                        string constraintName =
                            $"CK_{nameof(BookStock.AvailableStock)}_GreaterOrEqualZero";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            AddModelError(
                                "There isn't enough available stock at this Library" +
                                " to save the intended changes.");
                            return await ViewEditAsync(loan);
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Loan)}.");
            return await ViewEditAsync(loan);
        }


        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _loanRepository.SelectViewModelAsync(id.Value);
            if (model == null) return NotFound();

            // Success.
            return View(model);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan != null)
            {
                if (!loan.IsReturned)
                {
                    var bookStock = await _bookStockRepository
                        .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

                    if (bookStock == null) return NotFound();

                    bookStock.AvailableStock++;
                }

                // Delete Loan and update BookStock.
                await _loanRepository.DeleteAsync(loan);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReturnBook(int? loanId)
        {
            if (loanId == null) return NotFound();

            var model = await _loanRepository.SelectViewModelAsync(loanId.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost, ActionName("ReturnBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBookConfirmed(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) return NotFound();

            loan.IsReturned = true;
            loan.EndDate = DateTime.UtcNow;

            var bookStock = await _bookStockRepository
                .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

            if (bookStock == null) return NotFound();

            // BookStock is updated on SaveChangesAsync() inside UpdateAsync().
            bookStock.AvailableStock++;
            await _loanRepository.UpdateAsync(loan);

            return RedirectToAction(nameof(Index));
        }


        #region private helper methods

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "";
        }

        private async Task<ViewResult> ViewCreateAsync(CreateLoanViewModel model)
        {
            ViewBag.Role = await _userHelper.GetUserRoleAsync(GetCurrentUserName());

            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();
            ViewBag.Users = await _userHelper.GetComboCustomersAsync();

            if (User.IsInRole("Admin"))
            {
                ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            }

            return View(nameof(Create), model);
        }

        private async Task<ViewResult> ViewEditAsync(Loan loan)
        {
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Users = await _userHelper.GetComboCustomersAsync();

            // Success.
            return View(nameof(Edit), loan);
        }

        #endregion
    }
}
