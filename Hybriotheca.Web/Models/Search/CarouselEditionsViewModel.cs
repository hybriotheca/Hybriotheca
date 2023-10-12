using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Models.Search;

public class CarouselEditionsViewModel
{
    public List<BookEdition> Adventure { get; set; }

    public List<BookEdition> Fantasy { get; set; }

    public List<BookEdition> Romance { get; set; }

    public List<BookEdition> Science { get; set; }

    public List<BookEdition> Scifi { get; set; }
}
