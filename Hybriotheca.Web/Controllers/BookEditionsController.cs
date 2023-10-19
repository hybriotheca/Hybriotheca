using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookEditionsController : Controller
    {
        // Helpers.
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;

        // Repositories.
        private readonly IBookRepository _bookRepository;
        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserHelper _userHelper;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILoanRepository _loanRepository;

        public BookEditionsController(
            IBlobHelper blobHelper,
            IConverterHelper converterHelper,
            IBookRepository bookRepository,
            IBookEditionRepository bookEditionRepository,
            ICategoryRepository categoryRepository,
            IRatingRepository ratingRepository,
            IUserHelper userHelper,
            ISubscriptionRepository subscriptionRepository,
            ILoanRepository loanRepository)
        {
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            _bookRepository = bookRepository;
            _bookEditionRepository = bookEditionRepository;
            _categoryRepository = categoryRepository;
            _ratingRepository = ratingRepository;
            _userHelper = userHelper;
            _subscriptionRepository = subscriptionRepository;
            _loanRepository = loanRepository;
        }


        // GET: BookEditions
        public IActionResult Index()
        {
            return View(_bookEditionRepository.GetAll().OrderBy(b => b.EditionTitle).ThenBy(b => b.ISBN));
        }


        // GET: BookEditions/Create
        public async Task<IActionResult> Create()
        {
            return await ViewCreateAsync(new BookEditionViewModel());
        }

        // POST: BookEditions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookEditionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bookEdition = _converterHelper.ViewModelToBookEdition(model);

                // Upload Cover Image, if existent.
                if (model.CoverImageFile != null)
                {
                    try
                    {
                        bookEdition.CoverImageID =
                            await _blobHelper.UploadBlobAsync(model.CoverImageFile, "bookcovers");
                    }
                    catch
                    {
                        ViewBag.ErrorTitle = "Could not save Cover Image.";
                        return View("Error");
                    }
                }

                if (model.ePubFile != null)
                {
                    try
                    {
                        bookEdition.ePubID =
                            await _blobHelper.UploadEPUBAsync(model.ePubFile, "epub");
                    }
                    catch
                    {
                        ViewBag.ErrorTitle = "Could not save ePub file.";
                        return View("Error");
                    }
                }

                // Create.
                try
                {
                    await _bookEditionRepository.CreateAsync(bookEdition);

                    // Success.
                    TempData["Message"] = "Book Edition was created.";
                    return RedirectToAction(nameof(Index));
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(BookEdition)}.");
            return await ViewCreateAsync(model);
        }


        // GET: BookEditions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BookEditionNotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return BookEditionNotFound();

            var model = _converterHelper.BookEditionToViewModel(bookEdition);

            return await ViewEditAsync(model);
        }

        // POST: BookEditions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookEditionViewModel model)
        {

            (Guid CoverImageID, Guid ePubID) = _bookEditionRepository.GetCoverIDAndEpubID(model.ID);

            if (ModelState.IsValid)
            {
                var bookEdition = _converterHelper.ViewModelToBookEdition(model);

                // If new Cover Image was given, upload it and update BookEdition.
                // If not, just update and keep CoverImageID.
                try
                {
                    if (model.CoverImageFile != null)
                    {
                        // Upload new Cover Image.
                        try
                        {
                            // Delete previous cover image, if existent.
                            if (CoverImageID != Guid.Empty)
                                await _blobHelper.DeleteBlobAsync(
                                    CoverImageID.ToString(), "bookcovers");

                            // Upload new one.
                            bookEdition.CoverImageID =
                                await _blobHelper.UploadBlobAsync(model.CoverImageFile, "bookcovers");
                        }
                        catch
                        {
                            ViewBag.ErrorTitle = "Could not save Cover Image.";
                            return View("Error");
                        }

                    }
                    else
                    {
                        bookEdition.CoverImageID = CoverImageID;
                    }


                    if(model.ePubFile != null)
                    {
                        // Upload new ePub file.
                        try
                        {
                            // Delete previous ePub file, if existent.
                            if (ePubID != Guid.Empty)
                                await _blobHelper.DeleteEPUBAsync(
                                    ePubID.ToString(), "epub");

                            // Upload new one.
                            bookEdition.ePubID =
                                await _blobHelper.UploadEPUBAsync(model.ePubFile, "epub");
                        }
                        catch
                        {
                            ViewBag.ErrorTitle = "Could not save ePUB File.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        bookEdition.ePubID = ePubID;
                    }

                    // Update BookEdition, including new CoverImageID.
                    await _bookEditionRepository.UpdateAsync(bookEdition);

                    // Success.
                    TempData["Message"] = "Book Edition was updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookEditionRepository.ExistsAsync(bookEdition.ID))
                    {
                        return BookEditionNotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(BookEdition)}.");
            return await ViewEditAsync(model);
        }


        // GET: BookEditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BookEditionNotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return BookEditionNotFound();

            // Check dependent entities.
            var isConstrained = await _bookEditionRepository.IsConstrainedAsync(bookEdition.ID);
            if (isConstrained)
            {
                ViewBag.IsDeletable = false;
                ViewBag.Statement =
                    "You can't delete this Book Edition" +
                    " because there is at least 1 dependent entity using it." +
                    " Possible entities:" +
                    $" {nameof(BookStock)}," +
                    $" {nameof(Loan)}," +
                    $" {nameof(Rating)}," +
                    $" {nameof(Reservation)}." +
                    $" {nameof(Rating)}.";
            }
            else
            {
                ViewBag.IsDeletable = true;
            }

            // Success.
            return PartialView("_ModalDelete", bookEdition);
        }

        // POST: BookEditions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookEdition = await _bookEditionRepository.GetByIdAsync(id);
            if (bookEdition == null) return BookEditionNotFound();

            try
            {
                await _bookEditionRepository.DeleteAsync(bookEdition);

                // Success.
                TempData["Message"] = "Book Edition was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains(
                        "The DELETE statement conflicted with the REFERENCE constraint"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this Book edition.";
                        ViewBag.ErrorMessage =
                            "You can't delete this Base edition" +
                            " because there is at least 1 entity using it." +
                            " Possible entities:" +
                            $" {nameof(BookStock)}," +
                            $" {nameof(Loan)}," +
                            $" {nameof(Rating)}," +
                            $" {nameof(Reservation)}.";
                    }
                }
            }
            catch { }

            return View("Error");
        }



        // GET: BookDetails
        [Route("Book/Details/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> BookDetails(int id)
        {
            var bookEdition = await _bookEditionRepository.GetByIdWithRatingsAndBookAsync(id);
            var loggedUserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool loanLimitReached;
            bool alreadyBorrowed;
            List<Rating> ratings;

            if (bookEdition == null) return BookEditionNotFound();


            ratings = await _ratingRepository.GetRatingsByBookIDWithOtherUserRatings(bookEdition.ID);
            var user = await _userHelper.GetUserByIdAsync(loggedUserID);

            if (user != null)
            {
                loanLimitReached = await CheckIfLoanLimitReached(user);
                alreadyBorrowed = _bookEditionRepository.CheckIfBorrowed(bookEdition.ID, user.Id);
                ratings = ratings.OrderByDescending(s => s.UserID == loggedUserID).ToList();
            }
            else
            {
                loanLimitReached = false;
                alreadyBorrowed = false;
            }


            (List<BookEdition> OtherEditions, List<BookEdition> BooksYouMightEnjoy, List<BookEdition> OtherBooksBySameAuthor) = await _bookEditionRepository.GetBookDetailsCarouselAsync(bookEdition);

            var Book = new BookDetailsViewModel()
            {
                Book = bookEdition,
                Ratings = ratings,
                HasStock = _bookEditionRepository.CheckIfHasStock(bookEdition.ID),
                AlreadyBorrowed = alreadyBorrowed,
                LoanLimitReached = loanLimitReached,
                NewRating = new Rating()
                {
                    BookEditionID = bookEdition.ID,
                    UserID = loggedUserID,
                },
                OtherEditions = OtherEditions,
                BooksYouMightEnjoy = BooksYouMightEnjoy,
                OtherBooksBySameAuthor = OtherBooksBySameAuthor,
                
            };

            if (Book.Ratings is not null && Book.Ratings.Any())
            {

                Book.hasRating = Book.Ratings.Any(s => s.UserID == loggedUserID);

            }
            else
            {
                Book.hasRating = false;
            }

            return View(Book);
        }


        [Route("Book/Reader/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> EBookReader(int id)
        {
            var bookEdition = await _bookEditionRepository.GetByIdWithBookAsync(id);

            if (bookEdition == null) return BookEditionNotFound();

            if(string.IsNullOrEmpty(bookEdition.ePubFullPath)) return BookEditionEpubNotFound();

            ViewBag.ePubFile = bookEdition.ePubFullPath;

            ViewBag.Title = bookEdition.EditionTitle;

            ViewBag.Author = bookEdition.Book.Author;

            return View();
        }

        #region private helper methods

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private async Task<bool> CheckIfLoanLimitReached(AppUser user)
        {
            var userSubscription = await _subscriptionRepository.GetByIdAsync(user.SubscriptionID);
            var userLoans = await _loanRepository.CountUnreturnedWhereUserAsync(user.Id);

            if (userLoans >= userSubscription.MaxLoans)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public ViewResult BookEditionNotFound()
        {
            ViewBag.Title = "Book edition not found";
            ViewBag.ItemNotFound = "Book edition";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        public ViewResult BookEditionEpubNotFound()
        {
            ViewBag.Title = "ePUB file not found";
            ViewBag.ItemNotFound = "Book edition";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        public async Task<ViewResult> ViewCreateAsync(BookEditionViewModel model)
        {
            ViewBag.Books = (await _bookRepository.GetComboBooksAsync())
                .Prepend(new SelectListItem
                {
                    Text = $"Select a {nameof(Book)}",
                    Value = "0"
                });


            ViewBag.Categories = (await _categoryRepository.GetComboCategoriesAsync())
                .Prepend(new SelectListItem
                {
                    Text = $"Select a {nameof(Category)}",
                    Value = "0"
                });

            ViewBag.BookFormats = _bookEditionRepository.GetComboBookFormats();
            ViewBag.Languages = _bookEditionRepository.GetComboLanguages();

            return View(nameof(Create), model);
        }

        public async Task<ViewResult> ViewEditAsync(BookEditionViewModel model)
        {
            ViewBag.Books = await _bookRepository.GetComboBooksAsync();
            ViewBag.Categories = await _categoryRepository.GetComboCategoriesAsync();

            ViewBag.BookFormats = _bookEditionRepository.GetComboBookFormats();
            ViewBag.Languages = _bookEditionRepository.GetComboLanguages();

            return View(nameof(Edit), model);
        }

        #endregion private helper methods
    }
}
