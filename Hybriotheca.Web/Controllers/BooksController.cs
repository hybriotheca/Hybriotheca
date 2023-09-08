using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }


        // GET: Books
        public IActionResult Index()
        {
            return View(_bookRepository.GetAll());
            //return _context.Books != null ?
            //            View(await _context.Books.ToListAsync()) :
            //            Problem("Entity set 'DataContext.Books'  is null.");
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            // Success.
            return View(book);
        }


        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                await _bookRepository.CreateAsync(book);

                // Success.
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(Book)}.");
            return View(book);
        }


        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            // Success.
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _bookRepository.UpdateAsync(book);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookRepository.ExistsAsync(book.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(Book)}.");
            return View(book);
        }


        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            // Success.
            return PartialView("_ModalDelete", book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null)
            {
                await _bookRepository.DeleteAsync(book);
            }

            return RedirectToAction(nameof(Index));
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
