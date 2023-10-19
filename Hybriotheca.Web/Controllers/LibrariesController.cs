using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LibrariesController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public LibrariesController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }


        // GET: Libraries
        public IActionResult Index()
        {
            return View(_libraryRepository.GetAll().OrderBy(l => l.Name));
        }


        // GET: Libraries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Libraries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Library library)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _libraryRepository.CreateAsync(library);

                    // Success.
                    TempData["Message"] = "Library was created.";
                    return RedirectToAction(nameof(Index));
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(Library)}.");
            return View(library);
        }


        // GET: Libraries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return LibraryNotFound();

            var library = await _libraryRepository.GetByIdAsync(id.Value);
            if (library == null) return LibraryNotFound();

            // Success.
            return View(library);
        }

        // POST: Libraries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Library library)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _libraryRepository.UpdateAsync(library);

                    // Success.
                    TempData["Message"] = "Library was updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _libraryRepository.ExistsAsync(library.ID))
                    {
                        return LibraryNotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Library)}.");
            return View(library);
        }


        // GET: Libraries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return LibraryNotFound();

            var library = await _libraryRepository.GetByIdAsync(id.Value);
            if (library == null) return LibraryNotFound();

            var isConstrained = await _libraryRepository.IsConstrainedAsync(library.ID);
            if (isConstrained)
            {
                ViewBag.IsDeletable = false;
                ViewBag.Statement =
                    "You can't delete this Library" +
                    " because there is at least 1 dependent entity using it." +
                    " Possible entities:" +
                    $" {nameof(BookStock)}," +
                    $" {nameof(Loan)}," +
                    $" User.";
            }
            else
            {
                ViewBag.IsDeletable = true;
            }

            // Success.
            return PartialView("_ModalDelete", library);
        }

        // POST: Libraries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var library = await _libraryRepository.GetByIdAsync(id);
            if (library == null) return LibraryNotFound();

            try
            {
                await _libraryRepository.DeleteAsync(library);

                // Success.
                TempData["Message"] = "Library was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains(
                        "The DELETE statement conflicted with the REFERENCE constraint"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this Library";
                        ViewBag.ErrorMessage =
                            "You cannot delete this Library " +
                            "because there are Users whose Main Library is this one.";
                    }
                }
            }

            return View("Error");
        }


        #region private helper methods

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private ViewResult LibraryNotFound()
        {
            ViewBag.Title = "Library not found";
            ViewBag.ItemNotFound = "Library";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        #endregion private helper methods
    }
}
