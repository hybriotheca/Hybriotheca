using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
        private readonly IBookStockRepository _bookStockRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly IReservationRepository _reservationRepository;

        public BookEditionsController(
            IBlobHelper blobHelper, IConverterHelper converterHelper,
            IBookRepository bookRepository,
            IBookEditionRepository bookEditionRepository,
            IBookStockRepository bookStockRepository,
            ICategoryRepository categoryRepository,
            ILoanRepository loanRepository,
            IRatingRepository ratingRepository,
            IReservationRepository reservationRepository)
        {
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            _bookRepository = bookRepository;
            _bookEditionRepository = bookEditionRepository;
            _bookStockRepository = bookStockRepository;
            _categoryRepository = categoryRepository;
            _loanRepository = loanRepository;
            _ratingRepository = ratingRepository;
            _reservationRepository = reservationRepository;
        }


        // GET: BookEditions
        public IActionResult Index()
        {
            return View(_bookEditionRepository.GetAll());
        }


        // GET: BookEditions/Create
        public IActionResult Create()
        {
            ViewBag.Books = _bookRepository.GetComboBooks()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Book)}", Value = "0" });

            ViewBag.Categories = _categoryRepository.GetComboCategories()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Category)}", Value = "0" });

            return View(new BookEditionViewModel());
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

                // Create.
                try
                {
                    await _bookEditionRepository.CreateAsync(bookEdition);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(BookEdition)}.");

            ViewBag.Books = _bookRepository.GetComboBooks()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Book)}", Value = "0" });

            ViewBag.Categories = _categoryRepository.GetComboCategories()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Category)}", Value = "0" });

            return View(model);
        }


        // GET: BookEditions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return NotFound();

            var model = _converterHelper.BookEditionToViewModel(bookEdition);

            ViewBag.Books = _bookRepository.GetComboBooks();
            ViewBag.Categories = _categoryRepository.GetComboCategories();

            // Success.
            return View(model);
        }

        // POST: BookEditions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookEditionViewModel model)
        {
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
                            if (bookEdition.CoverImageID != Guid.Empty)
                                await _blobHelper.DeleteBlobAsync(
                                    bookEdition.CoverImageID.ToString(), "bookcovers");

                            // Upload new one.
                            bookEdition.CoverImageID =
                                await _blobHelper.UploadBlobAsync(model.CoverImageFile, "bookcovers");
                        }
                        catch
                        {
                            ViewBag.ErrorTitle = "Could not save Cover Image.";
                            return View("Error");
                        }
                        
                        // Update BookEdition, including new CoverImageID.
                        await _bookEditionRepository.UpdateAsync(bookEdition);
                    }
                    else
                    {
                        // No new Cover Image was given, so Update Book Edition but keep CoverImageID.
                        await _bookEditionRepository.UpdateKeepCoverImageAsync(bookEdition);
                    }

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookEditionRepository.ExistsAsync(bookEdition.ID))
                    {
                        return NotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(BookEdition)}.");

            ViewBag.Books = _bookRepository.GetComboBooks();
            ViewBag.Categories = _categoryRepository.GetComboCategories();

            return View(model);
        }


        // GET: BookEditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return NotFound();

            // Check dependent entities.
            var anyBookStock = await _bookStockRepository.AnyWhereBookEditionAsync(bookEdition.ID);
            var anyLoan = await _loanRepository.AnyWhereBookEditionAsync(bookEdition.ID);
            var anyRating = await _ratingRepository.AnyWhereBookEditionAsync(bookEdition.ID);
            var anyReservation = await _reservationRepository.AnyWhereBookEditionAsync(bookEdition.ID);

            if (anyBookStock || anyLoan || anyRating || anyReservation)
            {
                ViewBag.IsDeletable = false;
                ViewBag.Statement =
                    "You can't delete this Book Edition" +
                    " because there is at least 1 dependent entity using it." +
                    " Possible entities:" +
                    $" {nameof(BookStock)}," +
                    $" {nameof(Loan)}," +
                    $" {nameof(Rating)}," +
                    $" {nameof(Reservation)}.";
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
            if (bookEdition == null) return NotFound();

            try
            {
                await _bookEditionRepository.DeleteAsync(bookEdition);
                
                // Success.
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


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
