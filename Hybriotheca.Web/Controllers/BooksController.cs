﻿using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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
            return View(_bookRepository.GetAll().OrderBy(b => b.OriginalTitle));
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
                try
                {
                    await _bookRepository.CreateAsync(book);

                    // Success.
                    TempData["Message"] = "Base Book was created.";
                    return RedirectToAction(nameof(Index));
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(Book)}.");
            return View(book);
        }


        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BookNotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return BookNotFound();

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
                    TempData["Message"] = "Base Book was updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookRepository.ExistsAsync(book.ID))
                    {
                        return BookNotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Book)}.");
            return View(book);
        }


        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BookNotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return BookNotFound();

            ViewBag.IsDeletable = ! await _bookRepository.IsConstrainedAsync(id.Value);

            return PartialView("_ModalDelete", book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return BookNotFound();

            try
            {
                await _bookRepository.DeleteAsync(book);

                // Success.
                TempData["Message"] = "Base Book was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains("FK_BookEditions_Books_BookID"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this Base book.";
                        ViewBag.ErrorMessage =
                            "You can't delete this Base Book" +
                            " because there are Book Editions based on it.";
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

        private ViewResult BookNotFound()
        {
            ViewBag.Title = "Book not found";
            ViewBag.ItemNotFound = "Book";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }
    }
}
