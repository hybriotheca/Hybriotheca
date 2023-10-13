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
        private readonly ISubscriptionRepository _subscriptionRepository;

        public LoansController(
            IUserHelper userHelper,
            IBookEditionRepository bookEditionRepository,
            IBookStockRepository bookStockRepository,
            ILibraryRepository libraryRepository,
            ILoanRepository loanRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _userHelper = userHelper;
            _bookEditionRepository = bookEditionRepository;
            _bookStockRepository = bookStockRepository;
            _libraryRepository = libraryRepository;
            _loanRepository = loanRepository;
            _subscriptionRepository = subscriptionRepository;
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
            if (id == null) return LoanNotFound();

            var model = await _loanRepository.SelectViewModelAsync(id.Value);
            if (model == null) return LoanNotFound();

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
                if (libraryId == null) return LibraryNotFound();

                model.LibraryId = libraryId.Value;
            }

            if (ModelState.IsValid)
            {
                var bookStock = await _bookStockRepository
                    .GetByLibraryAndBookEditionAsync(model.LibraryId, model.BookEditionId);

                if (bookStock == null) return BookStockNotFound();

                // Check if user has reached limit of loans.

                var user = await _userHelper.GetUserByIdAsync(model.UserId);
                if (user == null) return UserNotFound();

                var subscription = await _subscriptionRepository.GetByIdAsync(user.SubscriptionID);
                if (subscription == null) return SubscriptionNotFound();

                var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
                if (userLoans >= subscription.MaxLoans)
                {
                    AddModelError("This user has reached the limit of loans.");
                    return await ViewCreateAsync(model);
                }

                // Create Loan.

                var dateToday = DateTime.UtcNow.Date;
                var loan = new Loan
                {
                    UserID = model.UserId,
                    LibraryID = model.LibraryId,
                    BookEditionID = model.BookEditionId,
                    ReservationID = null,
                    StartDate = dateToday,
                    EndDate = dateToday.AddDays(subscription.MaxLoanDays),
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
            if (id == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(id.Value);
            if (loan == null) return LoanNotFound();

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
                if (current == null) return LoanNotFound();

                if (!(current.IsReturned && loan.IsReturned))
                {
                    // Check count of user loans and update AvailableStocks.

                    // If the edited Loan is not returned,
                    // check user has reached limit of loans
                    // and decrement loan's BookStock AvailableStock.
                    if (!loan.IsReturned)
                    {
                        // Check user has reached limit of loans.
                        if (current.IsReturned || current.UserID != loan.UserID)
                        {
                            var user = await _userHelper.GetUserByIdAsync(loan.UserID);
                            if (user == null) return UserNotFound();

                            var subscription = await _subscriptionRepository
                                .GetByIdAsync(user.SubscriptionID);

                            if (subscription == null) return SubscriptionNotFound();

                            var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
                            if (userLoans >= subscription.MaxLoans)
                            {
                                AddModelError("This user has reached the limit of loans.");
                                return await ViewEditAsync(loan);
                            }
                        }

                        // Update BookStock.

                        var bookStockEdited = await _bookStockRepository
                            .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

                        if (bookStockEdited == null)
                        {
                            AddModelError("This book is not available in this Library.");
                            return await ViewEditAsync(loan);
                        }

                        bookStockEdited.AvailableStock--;
                    }

                    // If Loan was previously not returned, increment its BookStock AvailableStock.
                    if (!current.IsReturned)
                    {
                        var bookStockCurrent = await _bookStockRepository
                            .GetByLibraryAndBookEditionAsync(current.LibraryID, current.BookEditionID);

                        if (bookStockCurrent == null) return BookStockNotFound();

                        bookStockCurrent.AvailableStock++;
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
                        return LoanNotFound();
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
            if (id == null) return LoanNotFound();

            var model = await _loanRepository.SelectViewModelAsync(id.Value);
            if (model == null) return LoanNotFound();

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

                    if (bookStock == null) return BookStockNotFound();

                    bookStock.AvailableStock++;
                }

                // Delete Loan and update BookStock.
                await _loanRepository.DeleteAsync(loan);
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ReturnBook(int? loanId)
        {
            if (loanId == null) return LoanNotFound();

            var model = await _loanRepository.SelectViewModelAsync(loanId.Value);
            if (model == null) return LoanNotFound();

            return View(model);
        }

        [HttpPost, ActionName("ReturnBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBookConfirmed(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) return LoanNotFound();

            loan.IsReturned = true;
            loan.EndDate = DateTime.UtcNow;

            var bookStock = await _bookStockRepository
                .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

            if (bookStock == null) return BookStockNotFound();

            // BookStock is updated on SaveChangesAsync() inside UpdateAsync().
            bookStock.AvailableStock++;
            await _loanRepository.UpdateAsync(loan);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> HasUserReachedLoanLimit(string userId)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null) return Json("The user was not found.");

            var subscription = await _subscriptionRepository
                .GetByIdAsync(user.SubscriptionID);

            if (subscription == null) return Json("The user's subscription was not found");

            var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
            if (userLoans < subscription.MaxLoans)
            {
                return Json(false);
            }

            return Json(true);
        }

        #region private helper methods

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private ViewResult BookStockNotFound()
        {
            ViewBag.Title = "Book Stock not found";
            ViewBag.ItemNotFound = "Book Stock";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "";
        }

        private ViewResult LibraryNotFound()
        {
            ViewBag.Title = "Loan not found";
            ViewBag.ItemNotFound = "Loan";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private ViewResult LoanNotFound()
        {
            ViewBag.Title = "Loan not found";
            ViewBag.ItemNotFound = "Loan";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private ViewResult SubscriptionNotFound()
        {
            ViewBag.Title = "Subscription not found";
            ViewBag.ItemNotFound = "Subscription";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private ViewResult UserNotFound()
        {
            ViewBag.Title = "User not found";
            ViewBag.ItemNotFound = "User";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
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
