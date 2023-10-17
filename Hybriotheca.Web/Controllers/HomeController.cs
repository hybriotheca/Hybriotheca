﻿using Hybriotheca.Web.Data;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models;
using Hybriotheca.Web.Models.Account;
using Hybriotheca.Web.Models.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hybriotheca.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ILibraryRepository _libraryRepository;

        public HomeController(IUserHelper userHelper, ILibraryRepository libraryRepository)
        {
            _userHelper = userHelper;
            _libraryRepository = libraryRepository;
        }


        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult AdminPanel()
        {
            return View();
        }


        public async Task<IActionResult> UserProfile(string id)
        {
            var user = await _userHelper.GetUserByIdForProfileAsync(id);
                        
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            return View(user);
        }


        public IActionResult UserLoans()
        {
            return View();
        }


        public async Task<IActionResult> UserSettings()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserSettingsViewModel();

            model.UserViewModel = new UpdateUserViewModel
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


        public IActionResult BookDetails()
        {
            return View();
        }


        public IActionResult EbookReader()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult AccessDenied()
        {
            return View();
        }


        [Route("/Error/404")]
        public IActionResult NotFoundView()
        {
            return View("NotFound");
        }




        private async Task<UserViewModel?> GetModelForViewAsync(string id)
        {
            return await _userHelper.GetAllUsers()
                .Where(user => user.Id == id)
                .SelectUserViewModel()
                .SingleOrDefaultAsync();
        }
    }
}