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
    public class BooksInStockController : Controller
    {
        private readonly IUserHelper _userHelper;

        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IBookStockRepository _bookStockRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILoanRepository _loanRepository;

        public BooksInStockController(
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

        // GET: BooksInStock
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Index(BookStockViewModel? searchModel)
        {
            var model = new BookStockIndexViewModel
            {
                SearchModel = searchModel ?? new(),
            };

            // If any Id has been given, look for correspondent Book Stocks.
            if (searchModel != null && (searchModel.LibraryID > 0 || searchModel.BookEditionID > 0))
            {
                model.BookStocks = await _bookStockRepository
                    .SelectByLibraryAndBookEditionAsync(searchModel.LibraryID, searchModel.BookEditionID);
            }
            else
            {
                model.BookStocks = await _bookStockRepository.SelectLastCreatedAsync(25);
            }

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null)
                {
                    ViewBag.ErrorTitle = "Library not found";
                    ViewBag.ErrorMessage = "The main Library of the logged Librarian was not found.";
                    
                    return View("Error");
                }

                ViewBag.MainLibraryId = libraryId;
            }

            ViewBag.Role = await _userHelper.GetUserRoleAsync(GetCurrentUserName());

            return View(model);
        }


        // GET: BooksInStock/Create
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Create()
        {
            var bookStock = new BookStock();

            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null) return LibraryNotFound();

                bookStock.LibraryID = libraryId.Value;
            }

            return await ViewCreateAsync(bookStock);
        }

        // POST: BooksInStock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Create(BookStock bookStock)
        {
            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null) return LibraryNotFound();

                if (bookStock.LibraryID != libraryId)
                {
                    return View("Access Denied");
                }
            }

            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));
            ModelState.Remove(nameof(bookStock.AvailableStock));

            if (ModelState.IsValid)
            {
                // In case a Book Stock was (somehow) deleted,
                // when re-creating it count for loaned books.
                
                var loanedBooks = await _loanRepository
                    .CountBookEditionLoanedFromLibraryAsync(bookStock.LibraryID, bookStock.BookEditionID);

                bookStock.AvailableStock = bookStock.TotalStock - loanedBooks;

                try
                {
                    await _bookStockRepository.CreateAsync(bookStock);

                    // Success.
                    TempData["Message"] = "Book Stock was created.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is SqlException innerEx)
                    {
                        if (innerEx.Message.Contains("IX_BookEdition_Library"))
                        {
                            AddModelError("This Book Stock already exists.");
                            return await ViewCreateAsync(bookStock);
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(BookStock)}.");
            return await ViewCreateAsync(bookStock);
        }


        // GET: BooksInStock/Edit/5
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BookStockNotFound();

            var bookStock = await _bookStockRepository.GetByIdAsync(id.Value);
            if (bookStock == null) return BookStockNotFound();

            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null) return LibraryNotFound();

                if (bookStock.LibraryID != libraryId)
                {
                    return View("Access Denied");
                }
            }

            return await ViewEditAsync(bookStock);
        }

        // POST: BooksInStock/Edit/5
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookStock bookStock)
        {
            if (User.IsInRole("Librarian"))
            {
                var libraryId = await _userHelper.GetMainLibraryIdOfUserAsync(GetCurrentUserName());
                if (libraryId == null) return LibraryNotFound();

                if (bookStock.LibraryID != libraryId)
                {
                    return View("Access Denied");
                }
            }

            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));
            ModelState.Remove(nameof(bookStock.AvailableStock));

            if (ModelState.IsValid)
            {
                // Update available stock.
                int stockLoaned = await _loanRepository
                    .CountBookEditionLoanedFromLibraryAsync(bookStock.LibraryID, bookStock.BookEditionID);

                bookStock.AvailableStock = bookStock.TotalStock - stockLoaned;

                if (bookStock.AvailableStock < 0)
                {
                    AddModelError(
                        "Could not accept the value given for Total Stock." +
                        " The number of loaned books exceeds the given Stock.");

                    return await ViewEditAsync(bookStock);
                }

                try
                {
                    await _bookStockRepository.UpdateAsync(bookStock);

                    // Success.
                    TempData["Message"] = "Book Stock was updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookStockRepository.ExistsAsync(bookStock.ID))
                    {
                        return BookStockNotFound();
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
                                " to persist the intended changes.");
                            return await ViewEditAsync(bookStock);
                        }
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(BookStock)}.");
            return await ViewEditAsync(bookStock);
        }


        // GET: BooksInStock/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BookStockNotFound();

            var model = await _bookStockRepository.SelectViewModelAsync(id.Value);
            if (model == null) return BookStockNotFound();

            ViewBag.IsDeletable = ! await _loanRepository
                .AnyWhereLibraryAndBookEditionAsync(model.LibraryID, model.BookEditionID);

            // Success.
            return View("_ModalDelete", model);
        }

        // POST: BooksInStock/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(id);
            if (bookStock == null) return BookStockNotFound();

            if (bookStock.TotalStock > bookStock.AvailableStock)
            {
                AddModelError("Could not delete this Book Stock. There are loaned books.");
                return await Delete(id);
            }

            var anyLoan = await _loanRepository
                .AnyWhereLibraryAndBookEditionAsync(bookStock.LibraryID, bookStock.BookEditionID);

            if (anyLoan)
            {
                ViewBag.ErrorTitle = "Cannot delete this Book Stock";
                ViewBag.ErrorMessage =
                    "You cannot delete this Book Stock because there are Loans associated with it.";
                
                return View("Error");
            }

            try
            {
                await _bookStockRepository.DeleteAsync(bookStock);

                // Success.
                TempData["Message"] = "Book Stock was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch { }

            return View("Error");
        }


        public async Task<IActionResult> GetBookStockId(int libraryId, int bookEditionId)
        {
            return Json(await _bookStockRepository.GetBookStockIdAsync(libraryId, bookEditionId));
        }

        public async Task<IActionResult> CheckBookStockExists(int libraryId, int bookEditionId)
        {
            return Json(await _bookStockRepository.ExistsAsync(libraryId, bookEditionId));
        }

        public async Task<IActionResult> CheckIsBookAvailable(int libraryId, int bookEditionId)
        {
            return Json(
                await _bookStockRepository.IsBookAvailableInLibraryAsync(libraryId, bookEditionId));
        }

        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Decrement(int bookStockId)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(bookStockId);
            if (bookStock == null)
                return Json("Could not find book stock.");

            // Check there is at least 1 available book to dispense with.
            if (bookStock.AvailableStock < 1)
            {
                return Json("There isn't enough available stock.");
            }

            bookStock.TotalStock--;
            bookStock.AvailableStock--;

            try
            {
                await _bookStockRepository.UpdateAsync(bookStock);

                // Return the current values.
                return Json(new { Succeded = true, bookStock.TotalStock, bookStock.AvailableStock });
            }
            catch
            {
                return Json("error");
            }
        }

        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetMinBookStock(int libraryId, int bookEditionId)
        {
            return Json(
                await _bookStockRepository.GetUsedBookStockAsync(libraryId, bookEditionId));
        }

        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Increment(int bookStockId)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(bookStockId);
            if (bookStock == null)
                return Json("Could not find book stock.");

            bookStock.TotalStock++;
            bookStock.AvailableStock++;
            await _bookStockRepository.UpdateAsync(bookStock);

            // Return the current values.
            return Json(new { Succeded = true, bookStock.TotalStock, bookStock.AvailableStock });
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
            ViewBag.Title = "Library not found";
            ViewBag.ItemNotFound = "Library";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private async Task<ViewResult> ViewCreateAsync(BookStock? bookStock)
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(bookStock);
        }

        private async Task<ViewResult> ViewEditAsync(BookStock bookStock)
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(bookStock);
        }

        #endregion private helper methods
    }
}
