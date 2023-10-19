using System.Diagnostics;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models;
using Hybriotheca.Web.Models.Account;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hybriotheca.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IBookEditionRepository _bookEditionRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly ILoanRepository _loanRepository;

        public HomeController(IUserHelper userHelper, ILibraryRepository libraryRepository, IBookEditionRepository bookEditionRepository, IRatingRepository ratingRepository, ILoanRepository loanRepository)
        {
            _bookEditionRepository = bookEditionRepository;
            _ratingRepository = ratingRepository;
            _loanRepository = loanRepository;
            _userHelper = userHelper;
            _libraryRepository = libraryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _bookEditionRepository.GetHomeCarouselItems(10);

            return View(model);
        }


        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult AdminPanel()
        {
            double allUsers = _userHelper.GetAllUsers().Where(user => user.Role == "Customer").Count();
            int allBooks = _bookEditionRepository.GetAll().Count();
            int allLibraries = _libraryRepository.GetAll().Count();
            double allPremiumUsers = _userHelper.GetAllUsers().Where(user => user.Role == "Customer").Where(user => user.Subscription.Name == "Premium").Count();
            int allRatings = _ratingRepository.GetAll().Count();
            int allReviews = _ratingRepository.GetAll().Where(r => r.RatingBody.Length > 0).Count();
            int allLoans = _loanRepository.GetAll().Count();
            int allReserved = _loanRepository.GetAll().Where(l => l.Status == "Reserved").Count();
            int allActive = _loanRepository.GetAll().Where(l => l.Status == "Active").Count();


            ViewBag.AllUsers = allUsers;
            ViewBag.AllBooks = allBooks;
            ViewBag.AllLibraries = allLibraries;
            ViewBag.PremiumUsers = allPremiumUsers;
            ViewBag.UserPercentage = (allPremiumUsers / allUsers).ToString("P0");
            ViewBag.Ratings = allRatings;
            ViewBag.Reviews = allReviews;
            ViewBag.Loans = allLoans;
            ViewBag.ReservedLoans = allReserved;
            ViewBag.ActiveLoans = allActive;

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


        public async Task<IActionResult> UserLoans(string id)
        {
            var user = await _userHelper.GetUserByIdForLoanAsync(id);

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            return View(user);
        }


        public async Task<IActionResult> UserSettings()
        {
            var user = await _userHelper.SelectUserViewModel(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserSettingsViewModel();

            model.UserViewModel = user;

            ViewBag.Libraries = await _libraryRepository.GetComboLibrariesAsync();

            return View(model);
        }


        public IActionResult BookDetails()
        {
            return View();
        }


        public IActionResult Checkout()
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