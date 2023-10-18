using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Models.Home;

public class HomeCarouselViewModel
{
    public List<BookEdition> NewReleases { get; set; }

    public List<BookEdition> FeaturedBooks { get; set; }

    public List<BookEdition> Fantasy { get; set; }
}
