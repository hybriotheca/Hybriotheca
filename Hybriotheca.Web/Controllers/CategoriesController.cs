using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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
            return View(_categoryRepository.GetAll().OrderBy(c => c.Name));
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
                TempData["Message"] = "Category was created.";
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(Category)}.");
            return View(category);
        }


        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return CategoryNotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return CategoryNotFound();

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
                    TempData["Message"] = "Category was updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _categoryRepository.ExistsAsync(category.ID))
                    {
                        return CategoryNotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Category)}.");
            return View(category);
        }


        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return CategoryNotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return CategoryNotFound();

            ViewBag.IsDeletable = ! await _categoryRepository.IsConstrainedAsync(category.ID);

            // Success.
            return PartialView("_ModalDelete", category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return CategoryNotFound();
            
            try
            {
                await _categoryRepository.DeleteAsync(category);

                // Success.
                TempData["Message"] = "Category was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains("FK_BookEditions_Categories_CategoryID"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this Category.";
                        ViewBag.ErrorMessage =
                            "You can't delete this Category" +
                            " because there are Book Editions with it.";
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

        private ViewResult CategoryNotFound()
        {
            ViewBag.Title = "Category not found";
            ViewBag.ItemNotFound = "Category";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }
    }
}
