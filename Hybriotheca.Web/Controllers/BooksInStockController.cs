using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                model.BookStocks = await _bookStockRepository
                    .SelectByLibraryAndBookEditionAsListViewModelAsync(
                        searchModel.LibraryID, searchModel.BookEditionID);
            }
            else
                {
                model.BookStocks = await _bookStockRepository.SelectTop25AsListViewModelAsync();
            }

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View(model);
        }


        // GET: BooksInStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _bookStockRepository.SelectViewModelAsync(id.Value);
            if (model == null) return NotFound();

            // Success.
            return View(model);
        }


        // GET: BooksInStock/Create
        public async Task<IActionResult> Create()
        {
            return await ViewCreateAsync(null);
        }

        // POST: BooksInStock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookStock bookStock)
        {
            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));
            ModelState.Remove(nameof(bookStock.AvailableStock));

            if (ModelState.IsValid)
            {
                try
                {
                await _bookStockRepository.CreateAsync(bookStock);

                // Success.
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookStock = await _bookStockRepository.GetByIdAsync(id.Value);
            if (bookStock == null) return NotFound();

            return await ViewEditAsync(bookStock);
        }

        // POST: BooksInStock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookStock bookStock)
        {
            ModelState.Remove(nameof(bookStock.Library));
            ModelState.Remove(nameof(bookStock.BookEdition));
            ModelState.Remove(nameof(bookStock.AvailableStock));

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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _bookStockRepository.SelectViewModelAsync(id.Value);
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
            if (bookStock == null) return NotFound();

            if (bookStock.TotalStock > bookStock.AvailableStock)
            {
                AddModelError("Could not delete this Book Stock. There are loaned books.");
                return await Delete(id);
            }

            await _bookStockRepository.DeleteAsync(bookStock);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> CheckBookStockExists(int libraryId, int bookEditionId)
        {
            return Json(await _bookStockRepository.ExistsAsync(libraryId, bookEditionId));
        }

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

        public async Task<IActionResult> GetMinBookStock(int libraryId, int bookEditionId)
        {
            return Json(
                await _bookStockRepository.GetUsedBookStockAsync(libraryId, bookEditionId));
        }

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


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
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
    }
}
