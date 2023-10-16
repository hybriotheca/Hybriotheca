using Hybriotheca.Web.Data;
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
                // Check Start date is not before today.
                if (model.StartDate.Date < DateTime.UtcNow.Date)
                {
                    AddModelError("Start date cannot be previous to today.");
                    return await ViewCreateAsync(model);
                }

                var bookStock = await _bookStockRepository
                    .GetByLibraryAndBookEditionAsync(model.LibraryId, model.BookEditionId);

                if (bookStock == null) return BookStockNotFound();

                // Check if user has reached limit of loans.

                var user = await _userHelper.GetUserByIdAsync(model.UserId);
                if (user == null) return UserNotFound();

                var userSubscription = await _subscriptionRepository.GetByIdAsync(user.SubscriptionID);
                if (userSubscription == null) return SubscriptionNotFound();

                var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
                if (userLoans >= userSubscription.MaxLoans)
                {
                    AddModelError("This user has reached the limit of loans.");
                    return await ViewCreateAsync(model);
                }

                // Create Loan.
                var loan = new Loan
                {
                    UserID = model.UserId,
                    LibraryID = model.LibraryId,
                    BookEditionID = model.BookEditionId,
                    ReservationID = null,
                    CreateDate = DateTime.UtcNow.Date,
                    ReturnDate = null,
                };

                // Define Start Date and Status based on whether Check out is now or will be done later.
                if (model.WillCheckOutLater)
                {
                    loan.StartDate = model.StartDate;
                    loan.Status = BookLoanStatus.Reserved;
                }
                else
                {
                    loan.StartDate = loan.CreateDate;
                    loan.Status = BookLoanStatus.Active;
                }

                // Define Term limit date based on user's subsciption.
                loan.TermLimitDate = loan.StartDate.AddDays(userSubscription.MaxLoanDays);

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

                loan.CreateDate = current.CreateDate;

                if (!loan.IsReturned)
                {
                    loan.ReturnDate = null;
                }

                // Check count of user loans and update AvailableStocks.
                if (!(current.IsReturned && loan.IsReturned))
                {
                    // If the edited Loan's status is not "Returned",
                    // check user has reached limit of loans
                    // and decrement loan's BookStock AvailableStock.
                    if (!loan.IsReturned)
                    {
                        // If relevant, check user has reached limit of loans.
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


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(id.Value);
            if (loan == null) return LoanNotFound();

            return View("_ModalDelete", loan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null) return LoanNotFound();

            if (!loan.IsReturned)
            {
                var bookStock = await _bookStockRepository
                    .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

                if (bookStock == null) return BookStockNotFound();

                bookStock.AvailableStock++;
            }

            // Delete Loan and update BookStock.
            await _loanRepository.DeleteAsync(loan);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> HandOver(int? loanId)
        {
            if (loanId == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(loanId.Value);
            if (loan == null) return LoanNotFound();

            ViewBag.IsReserved = loan.IsReserved;

            return View("_ModalHandOver", loan);
        }

        [HttpPost, ActionName("HandOver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandOverConfirmed(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) return LoanNotFound();

            if (!loan.IsReserved)
            {
                ViewBag.ErrorTitle = "No reservation";
                ViewBag.ErrorMessage =
                    "The handover cannot be done because there isn't a related reservation.";

                return View("Error");
            }

            loan.Status = BookLoanStatus.Active;
            loan.StartDate = DateTime.UtcNow;

            try
            {
                await _loanRepository.UpdateAsync(loan);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _loanRepository.ExistsAsync(loan.ID))
                {
                    return LoanNotFound();
                }
            }
            catch { }

            return View("Error");
        }


        public async Task<IActionResult> ReturnBook(int? loanId)
        {
            if (loanId == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(loanId.Value);
            if (loan == null) return LoanNotFound();

            ViewBag.IsReturnable = loan.IsActive;

            return View("_ModalReturn", loan);
        }

        [HttpPost, ActionName("ReturnBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBookConfirmed(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) return LoanNotFound();

            if (!loan.IsActive)
            {
                ViewBag.ErrorTitle = "Loan is not active";
                ViewBag.ErrorMessage =
                    "This book cannot be returned because the related loan is not active.";

                return View("Error");
            }

            loan.Status = BookLoanStatus.Returned;
            loan.ReturnDate = DateTime.UtcNow;

            var bookStock = await _bookStockRepository
                .GetByLibraryAndBookEditionAsync(loan.LibraryID, loan.BookEditionID);

            if (bookStock == null) return BookStockNotFound();

            try
            {
                // BookStock is updated on SaveChangesAsync() inside UpdateAsync().
                bookStock.AvailableStock++;
                await _loanRepository.UpdateAsync(loan);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _loanRepository.ExistsAsync(loan.ID))
                {
                    return LoanNotFound();
                }
            }
            catch { }

            return View("Error");
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
            ViewBag.BookLoanStatuses = _loanRepository.GetComboBookLoanStatuses();
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Users = await _userHelper.GetComboCustomersAsync();

            // Success.
            return View(nameof(Edit), loan);
        }

        #endregion
    }
}
