using Hybriotheca.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Search;

public class SearchViewModel
{
    public List<BookEdition>? SearchResults { get; set; }

    public int NrPages { get; set; }

    public int PageSize { get; set; } = 6;

    public int TotalNrItems { get; set; }

    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;

    public List<string>? Categories { get; set; }

    public List<int>? Rating { get; set; }

    public List<string>? Formats { get; set; }

    public List<string>? Languages { get; set; }

    [Display(Name = "Available for loan")]
    public bool isAvailable { get; set; }

    public string? SortBy { get; set; }

    public List<string>? Year { get; set; }

    public string SearchURL { get; set; } = string.Empty;
}
