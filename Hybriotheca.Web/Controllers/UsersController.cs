using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // Helpers
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;

        // Repository
        private readonly ILibraryRepository _libraryRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public UsersController(
            IBlobHelper blobHelper,
            IConverterHelper converterHelper,
            IMailHelper mailHelper,
            IUserHelper userHelper,
            ILibraryRepository libraryRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            _mailHelper = mailHelper;
            _userHelper = userHelper;
            _libraryRepository = libraryRepository;
            _subscriptionRepository = subscriptionRepository;
        }


        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userHelper.GetAllUsersAsync();

            var models = new List<UserViewModel>();

            foreach (var user in users)
            {
                models.Add(await GetUserViewModelForViewAsync(user));
            }

            var roleOrder = new List<string> { "Admin", "Librarian", "Customer" };

            models = models
                .OrderBy(m => roleOrder.IndexOf(m.Role))
                .ThenBy(m => m.Email)
                .ToList();

            return View(models);
            //return _context.User != null ?
            //            View(await _context.User.ToListAsync()) :
            //            Problem("Entity set 'DataContext.User'  is null.");
        }


        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var model = await GetUserViewModelForViewAsync(user);
            model.SubscriptionName =
                await _subscriptionRepository.GetSubscriptionNameAsync(model.SubscriptionID)
                ?? "not found";

            return View(model);
        }


        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptions();

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            // The option to delete photo is not relevant when creating user
            // and could make ModelState invalid.
            ModelState.Remove(nameof(model.DeletePhoto));

            if (ModelState.IsValid)
            {
                var user = _converterHelper.ViewModelToUser(model);

                if (model.PhotoFile != null)
                {
                    user.PhotoId = await _blobHelper.UploadBlobAsync(model.PhotoFile, "userphotos");
                }

                var addUser = await _userHelper.AddUserAsync(user);
                if (addUser.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(user, model.Role);

                    string token = await _userHelper.GeneratePasswordResetTokenAsync(user);

                    string? tokenUrl = Url.Action(
                        action: nameof(AccountController.ResetPassword),
                        controller: "Account",
                        values: new { token },
                        protocol: HttpContext.Request.Scheme);

                    if (!string.IsNullOrEmpty(tokenUrl))
                    {
                        var sendPasswordResetEmail = _mailHelper.SendPasswordResetEmail(user, tokenUrl);
                        if (sendPasswordResetEmail)
                        {
                            TempData["Message"] =
                                $"An email has been sent to <i>{model.Email}</i> " +
                                $"with a link to reset password.";

                            // Success.
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // If it gets here, rollback user creation.
                    await _userHelper.DeleteUserAsync(user);
                }
            }

            AddModelError($"Could not create User.");

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptions();

            return View(model);
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var model = await GetUserViewModelForViewAsync(user);

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptions();

            // Success.
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userHelper.GetUserByIdAsync(model.Id);
                    if (user == null) return NotFound();

                    var isEmailChanged = user.Email != model.Email;
                    var oldEmail = user.Email;

                    // Keep email confirmed if already confirmed and has not changed.
                    if (user.EmailConfirmed && !isEmailChanged)
                    {
                        model.EmailConfirmed = true;
                    }

                    user = _converterHelper.ViewModelToUser(model, user);

                    if (model.PhotoFile != null)
                    {
                        user.PhotoId = await _blobHelper.UploadBlobAsync(model.PhotoFile, "userphotos");
                    }
                    else if (model.DeletePhoto)
                    {
                        await _blobHelper.DeleteBlobAsync(user.PhotoId.ToString(), "userphotos");
                        user.PhotoId = Guid.Empty;
                    }

                    var updateUser = await _userHelper.UpdateUserAsync(user);
                    if (updateUser.Succeeded)
                    {
                        if (!await _userHelper.IsUserInRoleAsync(user, model.Role))
                        {
                            await _userHelper.SetUserRoleAsync(user, model.Role);
                        }

                        if (isEmailChanged)
                        {
                            string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                            string? tokenUrl = Url.Action(
                                action: nameof(AccountController.ConfirmEmail),
                                controller: "Account",
                                values: new
                                {
                                    userid = user.Id,
                                    token
                                },
                                protocol: HttpContext.Request.Scheme);

                            if (!string.IsNullOrEmpty(tokenUrl))
                            {
                                var sendConfirmationEmail = _mailHelper.SendConfirmationEmail(user, tokenUrl);
                                if (sendConfirmationEmail)
                                {
                                    // Success.
                                    TempData["Message"] =
                                        $"User email address has changed." +
                                        $"A confirmation email has been sent to <i>{user.Email}</i>";

                                    return RedirectToAction(nameof(Index));
                                }
                            }

                            // Email address has changed but could not send reset password email.
                            TempData["Message"] = $"Could not send confirmation email.";
                            user.Email = oldEmail;

                            var revertUserEmail = await _userHelper.UpdateUserAsync(user);

                            if (revertUserEmail.Succeeded)
                            {
                                TempData["Message"] += "<br />User email has been reverted.";
                            }
                            else TempData["Message"] += "<br />Could not revert User email.";
                        }

                        // Success.
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _userHelper.UserExistsAsync(model.Id))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update User.");

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptions();

            return View(model);
        }


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var model = await GetUserViewModelForViewAsync(user);
            model.SubscriptionName =
                await _subscriptionRepository.GetSubscriptionNameAsync(model.SubscriptionID)
                ?? "not found";

            // Success.
            return PartialView("_ModalDelete", model);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userHelper.GetUserByIdAsync(id);
            if (user != null)
            {
                await _userHelper.DeleteUserAsync(user);

                if (user.PhotoId != Guid.Empty)
                {
                    await _blobHelper.DeleteBlobAsync(user.PhotoId.ToString(), "userphotos");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Admins()
        {
            var models = await GetModelsForUsersInRoleAsync("Admin");

            return View(nameof(Index), models);
        }

        public async Task<IActionResult> Customers()
        {
            var models = await GetModelsForUsersInRoleAsync("Customer");

            return View(nameof(Index), models);
        }

        public async Task<IActionResult> Librarians()
        {
            var models = await GetModelsForUsersInRoleAsync("Librarian");

            return View(nameof(Index), models);
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private async Task<IEnumerable<UserViewModel>> GetModelsForUsersInRoleAsync(string roleName)
        {
            var users = await _userHelper.GetUsersInRoleAsync(roleName);

            var models = new List<UserViewModel>();

            foreach (var user in users)
            {
                models.Add(await GetUserViewModelForViewAsync(user));
            }
            
            return models.OrderBy(m => m.Email);
        }

        private async Task<UserViewModel> GetUserViewModelForViewAsync(AppUser user)
        {
            var model = _converterHelper.UserToViewModel(user);
            model.Role = await _userHelper.GetUserRoleAsync(user);
            return model;
        }
    }
}
