using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Account;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hybriotheca.Web.Controllers
{
    public class AccountController : Controller
    {
        // Helpers
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;

        // Repository
        private readonly ILibraryRepository _libraryRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public AccountController(
            IBlobHelper blobHelper,
            IMailHelper mailHelper,
            IUserHelper userHelper,
            ILibraryRepository libraryRepository,
            ISubscriptionRepository subscriptionRepository
            )
        {
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _userHelper = userHelper;
            _libraryRepository = libraryRepository;
            _subscriptionRepository = subscriptionRepository;
        }


        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(GetCurrentUserName());
                if (user != null)
                {
                    var changePassword = await _userHelper.ChangePasswordAsync(
                        user, model.OldPassword, model.NewPassword);

                    if (changePassword.Succeeded)
                    {
                        ViewBag.UserMessage = "Password updated!";
                        return View();
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not update password.");
            return View();
        }


        public async Task<IActionResult> ConfirmEmail(string userid, string token)
        {
            if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorTitle = "This link does not work.";
                ViewBag.ErrorMessage = "This link cannot be used to Confirm email.";

                return View("Error");
            }

            var user = await _userHelper.GetUserByIdAsync(userid);
            if (user != null)
            {
                var confirmEmail = await _userHelper.ConfirmEmailAsync(user, token);
                if (confirmEmail.Succeeded)
                {
                    return View("EmailConfirmed");
                }
            }

            ViewBag.Message = "Could not confirm email.";
            return View();
        }


        public IActionResult ForgotPassword()
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    string token = await _userHelper.GeneratePasswordResetTokenAsync(user);

                    string? tokenUrl = Url.Action(
                        action: nameof(ResetPassword),
                        controller: "Account",
                        values: new { token },
                        protocol: HttpContext.Request.Scheme);

                    if (!string.IsNullOrEmpty(tokenUrl))
                    {
                        var sendPasswordResetEmail = _mailHelper.SendPasswordResetEmail(user, tokenUrl);
                        if (sendPasswordResetEmail)
                        {
                            // Success.

                            TempData["Message"] =
                                $"An email has been sent to <i>{model.Email}</i> " +
                                $"with a link to reset password.";

                            return RedirectToHomePage();
                        }
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not send password reset email.");
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userHelper.GetUserByEmailAsync(GetCurrentUserName());
            if (user == null) return View("Error");

            var model = new UpdateUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                HasPhoto = user.PhotoId != Guid.Empty,
                PhotoFullPath = user.PhotoFullPath,
                MainLibraryID = user.MainLibraryID ?? 0,
            };

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();

            return View(model);
        }


        public IActionResult Login()
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    var login = await _userHelper.LoginAsync(user, model.Password, model.RememberMe);
                    if (login.Succeeded)
                    {
                        string? returnUrl = Request.Query["ReturnUrl"];

                        // Redirect to return url.
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // Redirect to AdminPanel
                        if (User.IsInRole("Admin") || User.IsInRole("Librarian"))
                        {
                            RedirectToAction("AdminPanel", "Home");
                        }

                        return RedirectToHomePage();
                    }
                    else if (login.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Email confirmation is required.");
                        return View(model);
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not login.");
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToHomePage();
        }


        public IActionResult Register()
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            if (ModelState.IsValid)
            {
                // Check user already exists.
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, "This user is already registered.");
                    return View(model);
                }

                // Get default subscription for registered user.
                var defaultSubscriptionId =
                    await _subscriptionRepository.GetDefaultSubscriptionIdForNewUserAsync();

                user = new AppUser
                {
                    Role = "Customer",
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    SubscriptionID = defaultSubscriptionId,
                };

                var registerUser = await _userHelper.AddUserAsync(user, model.Password);
                if (registerUser.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(user, user.Role);

                    string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                    string? tokenUrl = Url.Action(
                        action: nameof(ConfirmEmail),
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

                            ViewBag.Message = "Hi there!" +
                                " In order to access the account, email confirmation is required." +
                                " A link has been sent to the registered email address.";

                            return View(nameof(ConfirmEmail));
                        }
                    }

                    // If it gets here, rollback user creation.
                    var deleteUser = await _userHelper.DeleteUserAsync(user);

                    if (!deleteUser.Succeeded)
                    {
                        return View("Error");
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not sign up.");
            return View();
        }


        public IActionResult ResetPassword(string token)
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (IsUserAuthenticated()) return RedirectToHomePage();

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    var resetPassword = await _userHelper.ResetPasswordAsync(
                        user, model.Token, model.Password);

                    if (resetPassword.Succeeded)
                    {
                        await _userHelper.ConfirmEmailAsync(user);

                        TempData["Message"] = "A new password has been set.";
                        return RedirectToAction(nameof(Login));
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not reset password.");
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(GetCurrentUserName());
                if (user != null)
                {
                    // Update user.

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.MainLibraryID = model.MainLibraryID > 0 ? model.MainLibraryID : null;

                    if (model.PhotoFile != null)
                    {
                        user.PhotoId = await _blobHelper.UploadBlobAsync(model.PhotoFile, "userphotos");
                    }

                    if (model.DeletePhoto && user.PhotoId != Guid.Empty)
                    {
                        await _blobHelper.DeleteBlobAsync(user.PhotoId.ToString(), "userphotos");
                        user.PhotoId = Guid.Empty;
                    }

                    var updateUser = await _userHelper.UpdateUserAsync(user);
                    if (updateUser.Succeeded)
                    {
                        TempData["Message"] = "The account details were updated.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Could not update account details.");
            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();
            return View(nameof(Index), model);
        }


        private IActionResult RedirectToHomePage()
        {
            return Redirect("/Home");
        }


        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "";
        }

        private bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

    }
}
