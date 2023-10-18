using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;

        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IBookStockRepository _bookStockRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public LoansController(
            IMailHelper mailHelper,
            IUserHelper userHelper,
            IBookEditionRepository bookEditionRepository,
            IBookStockRepository bookStockRepository,
            ILibraryRepository libraryRepository,
            ILoanRepository loanRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _mailHelper = mailHelper;
            _userHelper = userHelper;
            _bookEditionRepository = bookEditionRepository;
            _bookStockRepository = bookStockRepository;
            _libraryRepository = libraryRepository;
            _loanRepository = loanRepository;
            _subscriptionRepository = subscriptionRepository;
        }


        // GET: Loans
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Index()
        {
            var loans = await _loanRepository.SelectLastCreatedAsync(25);

            if (User.IsInRole("Admin"))
            {
                ViewBag.Role = "Admin";
                return View(loans);
            }

            if (User.IsInRole("Librarian"))
            {
                ViewBag.Role = "Librarian";
                return View(loans);
            }

            return View("Error");
        }


        // GET: Loans/Create
        [Authorize(Roles = "Admin,Librarian")]
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
        [Authorize(Roles = "Admin,Librarian")]
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
                if (userSubscription == null)
                {
                    ViewBag.ErrorTitle = "User subscription not found";
                    ViewBag.ErrorMessage =
                        "The user subscription was not found," +
                        " hence it's not possible to define the term limit date.";

                    return View("Error");
                }

                var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
                if (userLoans >= userSubscription.MaxLoans)
                {
                    AddModelError("This user has reached the limit of loans.");
                    return await ViewCreateAsync(model);
                }

                // Create Loan.
                var dateTimeUtcNowDate = DateTime.UtcNow.Date;
                var loan = new Loan
                {
                    UserID = model.UserId,
                    LibraryID = model.LibraryId,
                    BookEditionID = model.BookEditionId,
                    ReservationID = null,
                    CreateDate = dateTimeUtcNowDate,
                    ReturnDate = null,
                };

                // Define Start Date and Status based on whether Check out is now or will be done later.
                if (model.WillCheckOutLater)
                {
                    // Check reservation time limit.
                    if (DateTime.Compare(model.StartDate, dateTimeUtcNowDate.AddDays(7)) > 0)
                    {
                        AddModelError("The limit for reservations is 7 days.");
                        return await ViewCreateAsync(model);
                    }

                    loan.Status = BookLoanStatus.Reserved;
                    loan.StartDate = model.StartDate;
                }
                else
                {
                    loan.Status = BookLoanStatus.Active;
                    loan.StartDate = loan.CreateDate;
                }

                // Define Term limit date based on user's subsciption.
                loan.TermLimitDate = loan.StartDate.AddDays(userSubscription.MaxLoanDays);

                try
                {
                    // BookStock is updated on SaveChangesAsync() inside CreateAsync().
                    bookStock.AvailableStock--;
                    await _loanRepository.CreateAsync(loan);

                    var sendEmail = await _mailHelper.SendLoanCreatedEmail(loan, user.Email);
                    if (!sendEmail) TempData["Message"] = "Loan was created but email was not sent.";

                    // Success.
                    TempData["Message"] =
                        $"An email was sent to <i>{user.Email}</i> regarding the Loan.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is SqlException innerEx)
                    {
                        // Placeholder for each check.
                        string constraintName;

                        // Check AvailableStock not enough.
                        constraintName = $"CK_{nameof(BookStock.AvailableStock)}_GreaterOrEqualZero";
                        if (innerEx.Message.Contains(constraintName))
                        {
                            AddModelError(
                                "There isn't available Book Stock at this Library for this Loan.");
                            return await ViewCreateAsync(model);
                        }

                        // Check BookEdition reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.BookEditions)}" +
                            $"_{nameof(Loan.BookEditionID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return BookEditionNotFound();
                        }

                        // Check Library reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.Libraries)}" +
                            $"_{nameof(Loan.LibraryID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return LibraryNotFound();
                        }

                        // Check User reference.
                        if (innerEx.Message.Contains("FOREIGN KEY")
                            && innerEx.Message.Contains(nameof(Loan.UserID)))
                        {
                            return UserNotFound();
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(Loan)}.");
            return await ViewCreateAsync(model);
        }


        // GET: Loans/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(id.Value);
            if (loan == null) return LoanNotFound();

            return await ViewEditAsync(loan);
        }

        // POST: Loans/Edit/5
        [Authorize(Roles = "Admin")]
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
                if (DateTime.Compare(loan.TermLimitDate.Date, loan.StartDate.Date) < 0)
                {
                    AddModelError("The term limit date cannot be previous than the start date.");
                    return await ViewEditAsync(loan);
                }

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
                        // Placeholder for each check.
                        string constraintName;

                        // Check AvailableStock not enough.
                        constraintName = $"CK_{nameof(BookStock.AvailableStock)}_GreaterOrEqualZero";
                        if (innerEx.Message.Contains(constraintName))
                        {
                            AddModelError(
                                "There isn't enough available stock at this Library" +
                                " to save the intended changes.");
                            return await ViewEditAsync(loan);
                        }

                        // Check BookEdition reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.BookEditions)}" +
                            $"_{nameof(Loan.BookEditionID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return BookEditionNotFound();
                        }

                        // Check Library reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.Libraries)}" +
                            $"_{nameof(Loan.LibraryID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return LibraryNotFound();
                        }

                        // Check User reference.
                        if (innerEx.Message.Contains("FOREIGN KEY")
                            && innerEx.Message.Contains(nameof(Loan.UserID)))
                        {
                            return UserNotFound();
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Loan)}.");
            return await ViewEditAsync(loan);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(id.Value);
            if (loan == null) return LoanNotFound();

            return View("_ModalDelete", loan);
        }

        [Authorize(Roles = "Admin")]
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

            try
            {
                // Delete Loan and update BookStock.
                await _loanRepository.DeleteAsync(loan);

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


        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> HandOver(int? loanId)
        {
            if (loanId == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(loanId.Value);
            if (loan == null) return LoanNotFound();

            ViewBag.IsReserved = loan.IsReserved;

            return View("_ModalHandOver", loan);
        }

        [Authorize(Roles = "Admin,Librarian")]
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

            var user = await _userHelper.GetUserByIdAsync(loan.UserID);
            if (user == null) return UserNotFound();

            var userSubscription = await _subscriptionRepository.GetByIdAsync(user.SubscriptionID);
            if (userSubscription == null)
            {
                ViewBag.ErrorTitle = "User subscription not found";
                ViewBag.ErrorMessage =
                    "The handover cannot be done because user's subscription was not found.";

                return View("Error");
            }

            loan.Status = BookLoanStatus.Active;
            loan.StartDate = DateTime.UtcNow.Date;
            loan.TermLimitDate = loan.StartDate.AddDays(userSubscription.MaxLoanDays);

            try
            {
                await _loanRepository.UpdateAsync(loan);

                var sendEmail = await _mailHelper.SendLoanCheckedOutEmail(loan, user.Email);
                if (!sendEmail) TempData["Message"] = "Loan was checked out but email was not sent.";

                // Success.
                TempData["Message"] =
                        $"An email was sent to <i>{user.Email}</i> regarding the Loan handover.";
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


        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ReturnBook(int? loanId)
        {
            if (loanId == null) return LoanNotFound();

            var loan = await _loanRepository.GetByIdAsync(loanId.Value);
            if (loan == null) return LoanNotFound();

            ViewBag.IsReturnable = loan.IsActive;

            return View("_ModalReturn", loan);
        }

        [Authorize(Roles = "Admin,Librarian")]
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

                var user = await _userHelper.GetUserByIdAsync(loan.UserID);
                if (user != null)
                {
                    var sendEmail = await _mailHelper.SendLoanReturnedEmail(loan, user.Email);
                    if (sendEmail)
                    {
                        // Success.
                        TempData["Message"] =
                                $"An email was sent to <i>{user.Email}</i> regarding the Loan return.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                TempData["Message"] = "Book was returned but email was not sent.";
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


        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateLoanReservation(CreateLoanViewModel model)
        {
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.WillCheckOutLater));

            if (ModelState.IsValid)
            {
                var bookStock = await _bookStockRepository
                    .GetByLibraryAndBookEditionAsync(model.LibraryId, model.BookEditionId);

                if (bookStock == null) return BookStockNotFound();

                var dateTimeUtcNowDate = DateTime.UtcNow.Date;

                // Check Start date is not before today.
                if (model.StartDate.Date < dateTimeUtcNowDate)
                {
                    AddModelError("Start date cannot be previous to today.");
                    //return await ViewCreateAsync(model);
                }

                // Check reservation time limit.
                if (DateTime.Compare(model.StartDate, dateTimeUtcNowDate.AddDays(7)) > 0)
                {
                    AddModelError("The limit for reservations is 7 days.");
                    //return await ViewCreateAsync(model);
                }

                // Check user has reached limit of loans.

                var user = await _userHelper.GetUserByEmailAsync(GetCurrentUserName());
                if (user == null) return UserNotFound();

                var userSubscription = await _subscriptionRepository.GetByIdAsync(user.SubscriptionID);
                if (userSubscription == null)
                {
                    ViewBag.ErrorTitle = "User subscription not found";
                    ViewBag.ErrorMessage =
                        "The user subscription was not found," +
                        " hence it's not possible to define the term limit date.";

                    return View("Error");
                }

                var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);
                if (userLoans >= userSubscription.MaxLoans)
                {
                    AddModelError("This user has reached the limit of loans.");
                    //return await ViewCreateAsync(model);
                }

                var newLoan = new Loan
                {
                    UserID = user.Id,
                    LibraryID = model.LibraryId,
                    BookEditionID = model.BookEditionId,
                    ReservationID = null,
                    Status = BookLoanStatus.Reserved,
                    CreateDate = DateTime.UtcNow.Date,
                    StartDate = model.StartDate.Date,
                    TermLimitDate = model.StartDate.Date.AddDays(userSubscription.MaxLoanDays),
                    ReturnDate = null,
                };

                try
                {
                    // BookStock is updated on SaveChangesAsync() inside CreateAsync().
                    bookStock.AvailableStock--;
                    await _loanRepository.CreateAsync(newLoan);

                    var sendEmail = await _mailHelper.SendLoanCreatedEmail(newLoan, GetCurrentUserName());
                    if (!sendEmail) TempData["Message"] = "Loan was created but email was not sent.";

                    return Ok("Loan reservation was created.");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is SqlException innerEx)
                    {
                        // Placeholder for each check.
                        string constraintName;
                        
                        // Check AvailableStock not enough.
                        constraintName = $"CK_{nameof(BookStock.AvailableStock)}_GreaterOrEqualZero";
                        if (innerEx.Message.Contains(constraintName))
                        {
                            AddModelError("This book is currently not available at this Library.");
                            //return await ViewEditAsync(loan);
                        }

                        // Check BookEdition reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.BookEditions)}" +
                            $"_{nameof(Loan.BookEditionID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return BookEditionNotFound();
                        }

                        // Check Library reference.
                        constraintName =
                            $"FK_{nameof(DataContext.Loans)}" +
                            $"_{nameof(DataContext.Libraries)}" +
                            $"_{nameof(Loan.LibraryID)}.";

                        if (innerEx.Message.Contains(constraintName))
                        {
                            return LibraryNotFound();
                        }

                        // Check User reference.
                        if (innerEx.Message.Contains("FOREIGN KEY")
                            && innerEx.Message.Contains(nameof(Loan.UserID)))
                        {
                            return UserNotFound();
                        }
                    }
                }
                catch { }
            }

            // Temporary.
            return Json("Could not create Loan.");
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

        private ViewResult BookEditionNotFound()
        {
            ViewBag.Title = "Book Edition not found";
            ViewBag.ItemNotFound = "Book Edition";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
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
