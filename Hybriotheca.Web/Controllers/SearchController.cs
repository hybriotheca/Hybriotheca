using Htmx;
using Hybriotheca.Web.Models.Search;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hybriotheca.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBookEditionRepository _bookEditionRepository;

        public SearchController(IBookEditionRepository bookEditionRepository)
        {
            _bookEditionRepository = bookEditionRepository;
        }

        public IActionResult teste()
        {
            return View("IndexNew");
        }

        public async Task<IActionResult> Index(SearchViewModel model)
        {

            ViewBag.Categories = _bookEditionRepository.GetCheckBoxCategories();

            ViewBag.Formats = _bookEditionRepository.GetCheckBoxFormat();

            ViewBag.Languages = _bookEditionRepository.GetCheckBoxLang();

            ViewBag.PubYears = _bookEditionRepository.GetCheckBoxPubYear();

            ViewBag.Libraries = _bookEditionRepository.GetCheckBoxLibraries();

            var searchResults = await _bookEditionRepository.GetSearchResultsAsync(model);

            if (Request.IsHtmx())
            {
                //Response.Headers.Add("Vary", "HX-Request");

                Response.Htmx(h =>
                {
                    // we want to push the current url 
                    // into the history
                    h.PushUrl(searchResults.SearchURL + $"&page={model.Page}");
                });
            }

            return Request.IsHtmx()
            ? PartialView("_ResultsNew", searchResults)
            : View("IndexNew",searchResults);

        }

        // GET: SearchController
        public async Task<IActionResult> CarouselEditions()
        {
            var model = await _bookEditionRepository.GetCarouselEditionsAsync();

            return View(model);
        }

        public async Task<IActionResult> CarouselScroll(string category, int id)
        {

            var model = await _bookEditionRepository.CarouselEditionsInfiniteScrollAsync(category, id);

            return PartialView("_Carousel", model);
        }
    }
}
