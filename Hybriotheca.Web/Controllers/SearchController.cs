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

        // GET: SearchController
        public ActionResult CarouselEditions()
        {
            var model = _bookEditionRepository.GetCarouselEditions();

            return View(model);
        }

        public ActionResult CarouselScroll(string category ,int id)
        {

            var model = _bookEditionRepository.CarouselEditionsInfiniteScroll(category, id);

            return PartialView("_Carousel", model);
        }
    }
}
