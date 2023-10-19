using Htmx;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hybriotheca.Web.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IUserHelper _userHelper;

        public RatingsController(IRatingRepository ratingRepository,
            IBookEditionRepository bookEditionRepository, IUserHelper userHelper)
        {
            _ratingRepository = ratingRepository;
            _bookEditionRepository = bookEditionRepository;
            _userHelper = userHelper;
        }

        // GET: Ratings
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Index()
        {
            ViewBag.BookEditions = await _bookEditionRepository.GetComboBookEditionsAsync();

            return View();
        }


        // POST: Ratings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Librarian, Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rating rating)
        {
            if (!Request.IsHtmx()) { return BadRequest(); }

            ModelState.Remove("User");
            ModelState.Remove("BookEdition");

            if (ModelState.IsValid)
            {
                try
                {
                    await _ratingRepository.CreateAsync(rating);
                }
                catch (Exception)
                {

                    //Error
                    return Content("<div id=\"NoBookStockAvailable\" class=\"flex items-center p-4 mb-4 text-sm text-red-800 border border-red-300 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400 dark:border-red-800\" role=\"alert\">\r\n<svg class=\"flex-shrink-0 inline w-4 h-4 mr-3\" aria-hidden=\"true\" xmlns=\"http://www.w3.org/2000/svg\" fill=\"currentColor\" viewBox=\"0 0 20 20\">\r\n<path d=\"M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5ZM9.5 4a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM12 15H8a1 1 0 0 1 0-2h1v-3H8a1 1 0 0 1 0-2h2a1 1 0 0 1 1 1v4h1a1 1 0 0 1 0 2Z\" />\r\n</svg>\r\n<span class=\"sr-only\">Info</span>\r\n<div>\r\n        Unexpected error! Please try again.\r\n</div>\r\n</div>", "text/html");
                }

                //Get created rating by ID with other user ratings

                var newRating = await _ratingRepository.GetNewRatingByIDAsync(rating);

                if (newRating != null)
                {
                    return PartialView("_NewRating", newRating);
                }

                //Success
                return Content("<div class=\"p-4 mb-4 text-sm text-green-800 rounded-lg bg-green-50 dark:bg-gray-800 dark:text-green-400\" role=\"alert\">\r\n  <span class=\"font-medium\">You Rating has been created!</span> You can acess all your ratings in your profile.\r\n</div>", "text/html"); 
            }
            //Error
            return Content("<div id=\"NoBookStockAvailable\" class=\"flex items-center p-4 mb-4 text-sm text-red-800 border border-red-300 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400 dark:border-red-800\" role=\"alert\">\r\n<svg class=\"flex-shrink-0 inline w-4 h-4 mr-3\" aria-hidden=\"true\" xmlns=\"http://www.w3.org/2000/svg\" fill=\"currentColor\" viewBox=\"0 0 20 20\">\r\n<path d=\"M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5ZM9.5 4a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM12 15H8a1 1 0 0 1 0-2h1v-3H8a1 1 0 0 1 0-2h2a1 1 0 0 1 1 1v4h1a1 1 0 0 1 0 2Z\" />\r\n</svg>\r\n<span class=\"sr-only\">Info</span>\r\n<div>\r\n        Unexpected error! Please try again.\r\n</div>\r\n</div>", "text/html");
        }

        // GET: Ratings/Details/5
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _ratingRepository.GetByIDWithAll(id.Value);
            if (rating == null)
            {
                return NotFound();
            }


            return View(rating);
        }

        // GET: Ratings/Edit/5
        [Authorize(Roles = "Admin, Librarian, Customer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _ratingRepository.GetByIDWithAll(id.Value);
            if (rating == null)
            {
                return NotFound();
            }

            var loggedUserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (loggedUserID == null)
            {
                AddModelError($"Error! Could not retrieve user information.");
                return RedirectToAction(nameof(Index)); //TODO: Change to ratings page in user profile
            }

            if (loggedUserID != rating.UserID)
            {
                AddModelError($"Error! the rating you are trying to edit does not belong to you!");

                return RedirectToAction(nameof(Index)); //TODO: Change to ratings page in user profile
            }

            return View(rating);
        }

        // POST: Ratings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Librarian, Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Rating rating)
        {
            
            if (ModelState.IsValid)
            {

                var loggedUserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (loggedUserID == null)
                {
                    AddModelError($"Error! Could not retrieve user information.");
                    return RedirectToAction(nameof(Index)); //TODO: Change to ratings page in user profile
                }

                if (loggedUserID != rating.UserID)
                {
                    AddModelError($"Error! the rating you are trying to edit does not belong to you!");

                    return RedirectToAction(nameof(Index)); //TODO: Change to ratings page in user profile
                }

                try
                {
                    await _ratingRepository.UpdateAsync(rating);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _ratingRepository.ExistsAsync(rating.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); //TODO: Change to ratings page in user profile
            }

            AddModelError($"Error! Could not update Rating, please try again.");

            return View(rating);
        }

        // GET: Ratings/Delete/5
        [Authorize(Roles = "Admin, Librarian, Customer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!Request.IsHtmx()) { return BadRequest(); }

            if (id == null)
            {
                return NotFound();
            }

            var rating = await _ratingRepository.GetByIdAsync(id.Value);
            if (rating == null)
            {
                return NotFound();
            }

            Response.Htmx(h =>
            {
                h.WithTrigger("modalShow");
            });

            return PartialView("_ModalDelete", rating);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Librarian, Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);

            if (rating != null)
            {
                await _ratingRepository.DeleteAsync(rating);
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Librarian, Customer")]
        public async Task<IActionResult> Results(int? id)
        {
            if (!Request.IsHtmx()) { return BadRequest(); }

            if (id == null) return NotFound();

            var results = await _ratingRepository.GetRatingsByBookID(id.Value);

            if (results == null) return NotFound();

            return PartialView("_Results", results);
        }

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
