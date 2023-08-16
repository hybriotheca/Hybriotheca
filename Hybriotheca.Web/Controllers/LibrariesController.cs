using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
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
            return View(_libraryRepository.GetAll());
            //return _context.Libraries != null ?
            //            View(await _context.Libraries.ToListAsync()) :
            //            Problem("Entity set 'DataContext.Libraries'  is null.");
        }


        // GET: Libraries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var library = await _libraryRepository.GetByIdAsync(id.Value);
            if (library == null) return NotFound();

            // Success.
            return View(library);
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
                await _libraryRepository.CreateAsync(library);

                // Success.
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(Library)}.");
            return View(library);
        }


        // GET: Libraries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var library = await _libraryRepository.GetByIdAsync(id.Value);
            if (library == null) return NotFound();

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
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _libraryRepository.ExistsAsync(library.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(Library)}.");
            return View(library);
        }


        // GET: Libraries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var library = await _libraryRepository.GetByIdAsync(id.Value);
            if (library == null) return NotFound();

            // Success.
            return View(library);
        }

        // POST: Libraries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var library = await _libraryRepository.GetByIdAsync(id);
            if (library != null)
            {
                await _libraryRepository.DeleteAsync(library);
            }

            return RedirectToAction(nameof(Index));
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
