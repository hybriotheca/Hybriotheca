using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class BookEditionsController : Controller
    {
        // Helpers.
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;

        // Repositories.
        private readonly IBookRepository _bookRepository;
        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookEditionsController(
            IBlobHelper blobHelper, IConverterHelper converterHelper,
            IBookRepository bookRepository,
            IBookEditionRepository bookEditionRepository,
            ICategoryRepository categoryRepository)
        {
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            _bookRepository = bookRepository;
            _bookEditionRepository = bookEditionRepository;
            _categoryRepository = categoryRepository;
        }


        // GET: BookEditions
        public IActionResult Index()
        {
            return View(_bookEditionRepository.GetAll());
            //return _context.BookEditions != null ?
            //            View(await _context.BookEditions.ToListAsync()) :
            //            Problem("Entity set 'DataContext.BookEditions'  is null.");
        }


        // GET: BookEditions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return NotFound();

            // Success.
            return View(bookEdition);
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
                    bookEdition.CoverImageID =
                        await _blobHelper.UploadBlobAsync(model.CoverImageFile, "bookcovers");

                await _bookEditionRepository.CreateAsync(bookEdition);

                // Success.
                return RedirectToAction(nameof(Index));
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

            ViewBag.Books = _bookRepository.GetComboBooks()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Book)}", Value = "0" });

            ViewBag.Categories = _categoryRepository.GetComboCategories()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Category)}", Value = "0" });

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

                try
                {
                    // Check new Cover Image was given.
                    if (model.CoverImageFile != null)
                    {
                        // Upload new Cover Image.
                        bookEdition.CoverImageID =
                            await _blobHelper.UploadBlobAsync(model.CoverImageFile, "bookcovers");

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
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(BookEdition)}.");

            ViewBag.Books = _bookRepository.GetComboBooks()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Book)}", Value = "0" });

            ViewBag.Categories = _categoryRepository.GetComboCategories()
                .AsEnumerable()
                .Prepend(new SelectListItem { Text = $"Select a {nameof(Category)}", Value = "0" });

            return View(model);
        }


        // GET: BookEditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bookEdition = await _bookEditionRepository.GetByIdAsync(id.Value);
            if (bookEdition == null) return NotFound();

            // Success.
            return PartialView("_ModalDelete", bookEdition);
        }

        // POST: BookEditions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookEdition = await _bookEditionRepository.GetByIdAsync(id);
            if (bookEdition != null)
            {
                await _bookEditionRepository.DeleteAsync(bookEdition);
            }

            return RedirectToAction(nameof(Index));
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
