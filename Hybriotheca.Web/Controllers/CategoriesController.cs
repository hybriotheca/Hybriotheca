using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        // GET: Categories
        public IActionResult Index()
        {
            return View(_categoryRepository.GetAll());
            //return _context.Categories != null ?
            //            View(await _context.Categories.ToListAsync()) :
            //            Problem("Entity set 'DataContext.Categories'  is null.");
        }


        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            // Success.
            return View(category);
        }


        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.CreateAsync(category);

                // Success.
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(Category)}.");
            return View(category);
        }


        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            // Success.
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryRepository.UpdateAsync(category);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _categoryRepository.ExistsAsync(category.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(Category)}.");
            return View(category);
        }


        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            // Success.
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepository.DeleteAsync(category);
            }

            return RedirectToAction(nameof(Index));
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
