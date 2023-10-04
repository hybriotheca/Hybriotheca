using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class BooksInStockController : Controller
    {
        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IBookStockRepository _bookStockRepository;
        private readonly ILibraryRepository _libraryRepository;

        public BooksInStockController(
            IBookEditionRepository bookEditionRepository,
            IBookStockRepository bookStockRepository,
            ILibraryRepository libraryRepository)
        {
            _bookEditionRepository = bookEditionRepository;
            _bookStockRepository = bookStockRepository;
            _libraryRepository = libraryRepository;
        }

        // GET: BooksInStock
        public async Task<IActionResult> Index(BookStockViewModel? searchModel)
        {
            var model = new BookStockIndexViewModel
            {
                SearchModel = searchModel ?? new(),
            };

            // If any Id has been given, look for correspondent Book Stocks.
            if (searchModel != null && (searchModel.LibraryID > 0 || searchModel.BookEditionID > 0))
            {
                // Get IQueryable.
                var bookStocks = _bookStockRepository.GetAll();

                // Filter by Library, if id given.
                if (searchModel.LibraryID > 0)
                    bookStocks = bookStocks.Where(bookStock => bookStock.LibraryID == searchModel.LibraryID);

                // Filter by Book Edition, if id given.
                if (searchModel.BookEditionID > 0)
                    bookStocks = bookStocks.Where(bookStock => bookStock.BookEditionID == searchModel.BookEditionID);

                // Select List of BookStockViewModel.
                model.BookStocks = await bookStocks.Select(bookStock => new BookStockViewModel
                {
                    Id = bookStock.ID,
                    LibraryName = bookStock.Library.Name,
                    BookEditionTitle = bookStock.BookEdition.EditionTitle,
                    Quantity = bookStock.Quantity,
                }).ToListAsync();
            }

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(model);
        }


        // GET: BooksInStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await GetViewModelAsync(id.Value);
            if (model == null) return NotFound();

            // Success.
            return View(model);
        }


        // GET: BooksInStock/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View();
        }

        // POST: BooksInStock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookStock bookStock)
        {
            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));

            if (ModelState.IsValid)
            {
                await _bookStockRepository.CreateAsync(bookStock);

                // Success.
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(BookStock)}.");

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(bookStock);
        }


        // GET: BooksInStock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookStock = await _bookStockRepository.GetByIdAsync(id.Value);
            if (bookStock == null) return NotFound();

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            // Success.
            return View(bookStock);
        }

        // POST: BooksInStock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookStock bookStock)
        {
            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookStockRepository.UpdateAsync(bookStock);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookStockRepository.ExistsAsync(bookStock.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(BookStock)}.");

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(bookStock);
        }


        // GET: BooksInStock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await GetViewModelAsync(id.Value);
            if (model == null) return NotFound();

            // Success.
            return View(model);
        }

        // POST: BooksInStock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(id);
            if (bookStock != null)
            {
                await _bookStockRepository.DeleteAsync(bookStock);
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Decrement(int bookStockId)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(bookStockId);
            if (bookStock != null)
            {
                // TODO: Check number of loans where LibraryID == bookStock.LibraryID and 
                if (bookStock.Quantity > 0)
                {
                    bookStock.Quantity--;
                    await _bookStockRepository.UpdateAsync(bookStock);
                    return Json(bookStock.Quantity);
                }

                return Json("Can't have less than 0 books in stock.");
            }

            return Json("Could not find book stock.");
        }

        public async Task<IActionResult> Increment(int bookStockId)
        {
            var bookStock = await _bookStockRepository.GetByIdAsync(bookStockId);
            if (bookStock != null)
            {
                bookStock.Quantity++;
                await _bookStockRepository.UpdateAsync(bookStock);
                return Json(bookStock.Quantity);
            }
            return Json("Could not find book stock");
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private async Task<BookStockViewModel?> GetViewModelAsync(int id)
        {
            return await _bookStockRepository.GetAll()
                .Where(b => b.ID == id)
                .Select(b => new BookStockViewModel
                {
                    Id = b.ID,
                    LibraryName = b.Library.Name,
                    BookEditionTitle = b.BookEdition.EditionTitle,
                    Quantity = b.Quantity,
                })
                .SingleOrDefaultAsync();
        }
    }
}
