using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            var models = await _userHelper.GetAllUsers()
                .SelectUserViewModel()
                .ToListAsync();

            var roleOrder = new List<string> { "Admin", "Librarian", "Customer" };

            var orderedModels = models
                .OrderBy(model => roleOrder.IndexOf(model.Role))
                .ThenBy(model => model.Email);

            return View(orderedModels);
        }

        // GET: Users/Admins
        public async Task<IActionResult> Admins()
        {
            var models = await GetModelsOfRoleAsync("Admin");

            return View(nameof(Index), models);
        }

        // GET: Users/Customers
        public async Task<IActionResult> Customers()
        {
            var models = await GetModelsOfRoleAsync("Customer");

            return View(nameof(Index), models);
        }

        // GET: Users/Librarians
        public async Task<IActionResult> Librarians()
        {
            var models = await GetModelsOfRoleAsync("Librarian");

            return View(nameof(Index), models);
        }


        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var model = await GetModelForViewAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }


        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            return await ViewCreateAsync(null);
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

                // Upload Photo if given.
                if (model.PhotoFile != null)
                {
                    try
                    {
                        user.PhotoId = await _blobHelper.UploadBlobAsync(model.PhotoFile, "userphotos");
                    }
                    catch
                    {
                        AddModelError("Could not save photo. The user was not created.");
                        return await ViewCreateAsync(model);
                    }
                }

                // Add user and send email.
                try
                {
                    var addUser = await _userHelper.AddUserAsync(user);
                    if (addUser.Succeeded)
                    {
                        await _userHelper.AddUserToRoleAsync(user, user.Role);

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
                        var deleteUser = await _userHelper.DeleteUserAsync(user);

                        if (!deleteUser.Succeeded)
                        {
                            return View("Error");
                        }

                        AddModelError("Could not send confirmation email to User. User was not created.");
                        return await ViewCreateAsync(model);
                    }
                }
                catch { }
            }

            AddModelError("Could not create User.");
            return await ViewCreateAsync(model);
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var model = await GetModelForViewAsync(id);
            if (model == null) return NotFound();

            return await ViewEditAsync(model);
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
                        if (!await _userHelper.IsUserInRoleAsync(user, user.Role))
                        {
                            await _userHelper.SetUserRoleAsync(user, user.Role);
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
                }
                catch { }
            }

            AddModelError("Could not update User.");
            return await ViewEditAsync(model);
        }


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var model = await GetModelForViewAsync(id);
            if (model == null) return NotFound();

            // Check if this user is the last admin.
            if (model.Role == "Admin")
            {
                var lastAdmin = !await _userHelper.IsThereAnyOtherAdminAsync(id);
                if (lastAdmin)
                {
                    ViewBag.IsDeletable = false;
                    ViewBag.Statement = "You cannot delete this User because there isn't any other Admin.";
                    return PartialView("_ModalDelete", model);
                }
            }

            // Check if there is any entity dependent on this user.
            var isConstrained = await _userHelper.IsConstrainedAsync(id);
            if (isConstrained)
            {
                ViewBag.IsDeletable = false;
                ViewBag.Statement =
                    "You cannot delete this User because there are entities related to it." +
                    " Possible entities:" +
                    $" {nameof(Loan)}," +
                    $" {nameof(Rating)}," +
                    $" {nameof(Reservation)}.";

                return PartialView("_ModalDelete", model);
            }

            // User is deletable.
            ViewBag.IsDeletable = true;
            return PartialView("_ModalDelete", model);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            try
            {
                var deleteUser = await _userHelper.DeleteUserAsync(user);

                if (!deleteUser.Succeeded) return View("Error");
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains(
                        "The DELETE statement conflicted with the REFERENCE constraint"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this User.";
                        ViewBag.ErrorMessage =
                            "You can't delete this User" +
                            " because there is at least 1 entity related to it." +
                            " Possible entities:" +
                            $" {nameof(Loan)}," +
                            $" {nameof(Rating)}," +
                            $" {nameof(Reservation)}.";
                    }
                }
            }
            catch { return View("Error"); }

            if (user.PhotoId != Guid.Empty)
            {
                try
                {
                    await _blobHelper.DeleteBlobAsync(user.PhotoId.ToString(), "userphotos");
                }
                catch
                {
                    ViewBag.ErrorTitle = "The image was not deleted.";
                    return View("Error");
                }
            }

            return RedirectToAction(nameof(Index));
        }


        #region private helper methods

        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private async Task<UserViewModel?> GetModelForViewAsync(string id)
        {
            return await _userHelper.GetAllUsers()
                .Where(user => user.Id == id)
                .SelectUserViewModel()
                .SingleOrDefaultAsync();
        }

        private async Task<IEnumerable<UserViewModel>> GetModelsOfRoleAsync(string roleName)
        {
            return await _userHelper.GetAllUsers()
                .Where(user => user.Role == roleName)
                .SelectUserViewModel()
                .OrderBy(user => user.Email)
                .ToListAsync();
        }

        private async Task<ViewResult> ViewCreateAsync(UserViewModel? model)
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptionsAsync();

            return View(nameof(Create), model);
        }

        private async Task<ViewResult> ViewEditAsync(UserViewModel model)
        {
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            ViewBag.Roles = await _userHelper.GetComboRolesAsync();
            ViewBag.Subscriptions = await _subscriptionRepository.GetComboSubscriptionsAsync();

            return View(nameof(Edit), model);
        }

        #endregion private helper methods
    }
}
