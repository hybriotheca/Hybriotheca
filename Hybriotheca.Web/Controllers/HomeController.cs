using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models;
using Hybriotheca.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hybriotheca.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserHelper _userHelper;

        public HomeController(IUserHelper _userHelper)
        {
            this._userHelper = _userHelper;
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
            var user = await _userHelper.GetUserByIdAsync(id);
            
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

        public IActionResult UserSettings()
        {
            return View();
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
    }
}