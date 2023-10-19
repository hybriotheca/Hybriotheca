using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Home;
using Hybriotheca.Web.Models.Search;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Hybriotheca.Web.Repositories;

public class BookEditionRepository : GenericRepository<BookEdition>, IBookEditionRepository
{
    private readonly DataContext _dataContext;
    private bool _hasExistingParams = false;

    public BookEditionRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<IEnumerable<SelectListItem>> GetComboBookEditionsAsync()
    {
        return await _dataContext.BookEditions
            .Select(bookEdition => new SelectListItem
            {
                Text = bookEdition.EditionTitle,
                Value = bookEdition.ID.ToString(),
            }).ToListAsync();
    }

    public async Task<bool> IsConstrainedAsync(int id)
    {
        return await _dataContext.BookEditions
            .Where(bookEdition => bookEdition.ID == id)
            .AnyAsync(bookEdition =>
                bookEdition.BooksInStock.Any()
                || bookEdition.Loans.Any()
                || bookEdition.Ratings.Any()
                || bookEdition.Reservations.Any());
    }

    public async Task<CarouselEditionsViewModel> GetCarouselEditionsAsync()
    {
        var adventure = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Adventure").Take(4).AsNoTracking().ToListAsync();

        var fantasy = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Fantasy").Take(4).AsNoTracking().ToListAsync();

        var romance = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Romance").Take(4).AsNoTracking().ToListAsync();

        var science = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Science").Take(4).AsNoTracking().ToListAsync();

        var scifi = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == "Sci-fi").Take(4).AsNoTracking().ToListAsync();

        CarouselEditionsViewModel CarouselEditions = new()
        {
            Adventure = adventure,
            Fantasy = fantasy,
            Romance = romance,
            Science = science,
            Scifi = scifi
        };

        return CarouselEditions;
    }

    public async Task<List<BookEdition>> CarouselEditionsInfiniteScrollAsync(string category, int lastEditionID)
    {
        if (string.IsNullOrEmpty(category))
        {
            return new List<BookEdition>();
        }

        var list = await _dataContext.BookEditions.Include(s => s.Category).AsSplitQuery().Where(x => x.Category.Name == category).Where(s => s.ID > lastEditionID).Take(4).AsNoTracking().ToListAsync();

        return list;
    }

    public async Task<SearchViewModel> GetSearchResultsAsync(SearchViewModel viewModel)
    {

        int pageSize = viewModel.PageSize;
        int currentPage = viewModel.Page;

        string searchUrl = "/Search?";
        var editionsQuery = _dataContext.BookEditions
            .AsNoTracking()
            .Include(a => a.Book)
            .Include(s => s.Ratings)
            .AsSplitQuery();

        if (viewModel.Categories is not null && viewModel.Categories.Any())
        {
            editionsQuery = editionsQuery
                .Include(s => s.Category)
                .AsSplitQuery()
                .Where(x => EF.Functions.Contains(viewModel.Categories, x.Category.Name));

            for (int i = 0; i < viewModel.Categories.Count; i++)
            {
                if (i == 0)
                {
                    searchUrl = string.Concat(searchUrl, $"categories={HttpUtility.UrlEncode(viewModel.Categories[0])}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&categories={HttpUtility.UrlEncode(viewModel.Categories[i])}");
                }
            }

            _hasExistingParams = true;
        }

        if (viewModel.Rating is not null && viewModel.Rating.Any())
        {
            editionsQuery = editionsQuery.Where(x => viewModel.Rating.Contains(((int)x.Ratings.Average(w => w.BookRating))));

            //var test = editionsQuery.Include(s => s.Ratings).AsSplitQuery().FirstOrDefault(x => x.ID == 1);


            for (int i = 0; i < viewModel.Rating.Count; i++)
            {
                if (i == 0 && !_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"rating={viewModel.Rating[0]}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&rating={viewModel.Rating[i]}");
                }
            }

            _hasExistingParams = true;

        }

        if (viewModel.Formats is not null && viewModel.Formats.Any())
        {
            editionsQuery = editionsQuery.Where(x => viewModel.Formats.Contains(x.BookFormat));

            for (int i = 0; i < viewModel.Formats.Count; i++)
            {
                if (i == 0 && !_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"formats={HttpUtility.UrlEncode(viewModel.Formats[0])}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&formats={HttpUtility.UrlEncode(viewModel.Formats[i])}");
                }
            }

            _hasExistingParams = true;
        }

        if (viewModel.Languages is not null && viewModel.Languages.Any())
        {
            editionsQuery = editionsQuery.Where(x => viewModel.Languages.Contains(x.Language));

            for (int i = 0; i < viewModel.Languages.Count; i++)
            {
                if (i == 0 && !_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"languages={HttpUtility.UrlEncode(viewModel.Languages[0])}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&languages={HttpUtility.UrlEncode(viewModel.Languages[i])}");
                }
            }

            _hasExistingParams = true;
        }

        if (viewModel.Libraries is not null && viewModel.Libraries.Any())
        {
            editionsQuery = editionsQuery.Include(s => s.BooksInStock).AsSplitQuery().Where(x => x.BooksInStock.Any( a => viewModel.Libraries.Contains( a.Library.Name)));

            for (int i = 0; i < viewModel.Libraries.Count; i++)
            {
                if (i == 0 && !_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"libraries={HttpUtility.UrlEncode(viewModel.Libraries[0])}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&libraries={HttpUtility.UrlEncode(viewModel.Libraries[i])}");
                }
            }

            _hasExistingParams = true;
        }

        if (viewModel.Year is not null && viewModel.Year.Any())
        {
            editionsQuery = editionsQuery.Where(x => viewModel.Year.Contains(x.PublishDate.Year.ToString()));

            for (int i = 0; i < viewModel.Year.Count; i++)
            {
                if (i == 0 && !_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"year={viewModel.Year[0]}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&year={viewModel.Year[i]}");
                }
            }

            _hasExistingParams = true;
        }

        if (!string.IsNullOrWhiteSpace(viewModel.SearchTerm))
        {
            editionsQuery = editionsQuery.Where(s =>
                EF.Functions.Contains(s.EditionTitle, viewModel.SearchTerm) ||
                EF.Functions.Contains(s.Book.Author, viewModel.SearchTerm) ||
                (s.ISBN != null && EF.Functions.Contains(s.ISBN, viewModel.SearchTerm)) ||
                EF.Functions.Contains(s.Publisher, viewModel.SearchTerm));

            if (!_hasExistingParams)
            {
                searchUrl = string.Concat(searchUrl, $"searchTerm={HttpUtility.UrlEncode(viewModel.SearchTerm)}");
            }
            else
            {
                searchUrl = string.Concat(searchUrl, $"&searchTerm={HttpUtility.UrlEncode(viewModel.SearchTerm)}");
            }

            _hasExistingParams = true;
        }


        if (viewModel.isAvailable)
        {
            editionsQuery = editionsQuery.Include(s => s.BooksInStock).AsSplitQuery().Where(x => x.BooksInStock.Any(a => a.AvailableStock >= 1));

            if (!_hasExistingParams)
            {
                searchUrl = string.Concat(searchUrl, $"isAvailable={viewModel.isAvailable}");
            }
            else
            {
                searchUrl = string.Concat(searchUrl, $"&isAvailable={viewModel.isAvailable}");
            }

            _hasExistingParams = true;
        }


        if (pageSize != 6)
        {
            if (_hasExistingParams)
            {
                searchUrl = string.Concat(searchUrl, $"&pageSize={viewModel.PageSize}");
            }
            else
            {
                searchUrl = string.Concat(searchUrl, $"pageSize={viewModel.PageSize}");

                _hasExistingParams = true;
            }

        }

        switch (viewModel.SortBy)
        {
            case "relevance":
                editionsQuery = editionsQuery.OrderByDescending(s => s.ID);
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
            case "title_A_Z":
                editionsQuery = editionsQuery.OrderBy(s => s.EditionTitle);
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
            case "title_Z_A":
                editionsQuery = editionsQuery.OrderByDescending(s => s.EditionTitle);
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
            case "rating":
                editionsQuery = editionsQuery.OrderByDescending(s => ((int)s.Ratings.Average(w => w.BookRating)));  
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
            case "pub_date":
                editionsQuery = editionsQuery.OrderByDescending(s => s.PublishDate);
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
            default:
                editionsQuery = editionsQuery.OrderByDescending(s => s.ID);
                if (_hasExistingParams)
                {
                    searchUrl = string.Concat(searchUrl, $"&sortby={viewModel.SortBy}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"sortby={viewModel.SortBy}");
                }
                break;
        }

        var totalNrITems = editionsQuery.Count();
        int nrPages = (int)Math.Ceiling((double)totalNrITems / pageSize);
        var results = await editionsQuery.Skip((currentPage - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();

        var model = new SearchViewModel()
        {
            SearchResults = results,
            NrPages = nrPages,
            TotalNrItems = totalNrITems,
            SearchTerm = viewModel.SearchTerm,
            Page = currentPage,
            PageSize = pageSize,
            Categories = viewModel.Categories,
            Rating = viewModel.Rating,
            Formats = viewModel.Formats,
            Languages = viewModel.Languages,
            Year = viewModel.Year,
            SearchURL = searchUrl,
            SortBy = viewModel.SortBy,
            Libraries = viewModel.Libraries,
        };
        return model;
    }


    public IEnumerable<string> GetCheckBoxCategories()
    {

        var checkBoxCategories = _dataContext.Categories.Select(c => c.Name);


        return checkBoxCategories;
    }

    public IEnumerable<string> GetCheckBoxFormat()
    {

        var checkBoxFormat = _dataContext.BookEditions.Select(c => c.BookFormat)
        .Distinct();


        return checkBoxFormat;
    }

    public IEnumerable<string> GetCheckBoxLang()
    {
        var checkBoxLang = _dataContext.BookEditions.Select(c => c.Language)
        .Distinct();


        return checkBoxLang;
    }

    public IEnumerable<string> GetCheckBoxPubYear()
    {
        var comboPubYear = _dataContext.BookEditions.Select(c => c.PublishDate.Year.ToString())
        .Distinct();

        return comboPubYear;
    }

    public IEnumerable<string> GetCheckBoxLibraries()
    {
        var comboLibraries = _dataContext.Libraries.Select(c => c.Name)
        .Distinct();

        return comboLibraries;
    }

    public (Guid CoverID, Guid ePubID) GetCoverIDAndEpubID(int ID)
    {
        Guid guid1 = _dataContext.BookEditions.Where(x => x.ID == ID).Select(s => s.CoverImageID).FirstOrDefault();
        Guid guid2 = _dataContext.BookEditions.Where(x => x.ID == ID).Select(s => s.ePubID).FirstOrDefault();

        return (CoverID: guid1, ePubID: guid2);
    }

    public List<SelectListItem> GetComboLanguages()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "Select a language", Value = "" },
            new SelectListItem { Text = "English", Value = "English" },
            new SelectListItem { Text = "Chinese", Value = "Chinese" },
            new SelectListItem { Text = "Spanish", Value = "Spanish" },
            new SelectListItem { Text = "French", Value = "French" },
            new SelectListItem { Text = "German", Value = "German" },
            new SelectListItem { Text = "Portuguese", Value = "Portuguese" },
            new SelectListItem { Text = "Italian", Value = "Italian" },
            new SelectListItem { Text = "Russian", Value = "Russian" },

        };
    }

    public List<SelectListItem> GetComboBookFormats()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "Select a book format", Value = "" },
            new SelectListItem { Text = "Paperback", Value = "Paperback" },
            new SelectListItem { Text = "Hardcover", Value = "Hardcover" },
            new SelectListItem { Text = "Board Book", Value = "Board Book" },
            new SelectListItem { Text = "Wire-Bound", Value = "Wire-Bound" },
            new SelectListItem { Text = "Leather-Bound", Value = "Leather-Bound" },
            new SelectListItem { Text = "Cloth-Bound", Value = "Cloth-Bound" },
            new SelectListItem { Text = "Italian", Value = "Italian" },
            new SelectListItem { Text = "Pocket-Sized", Value = "Pocket-Sized" },

        };
    }

    public async Task<BookEdition> GetByIdWithRatingsAndBookAsync(int id)
    {
        return await _dataContext.BookEditions.Include(s => s.Ratings).Include(q => q.Book).Include(a => a.Category).AsSplitQuery().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id);
    }


    public async Task<(List<BookEdition> OtherEditions, List<BookEdition> BooksYouMightEnjoy, List<BookEdition> OtherBooksBySameAuthor)> GetBookDetailsCarouselAsync(BookEdition book)
    {
        var list1 = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).AsSplitQuery().Where(x => x.BookID == book.BookID && x.ID != book.ID).AsNoTracking().Take(10).ToListAsync();
        var tempList = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).AsSplitQuery().Where(x => x.CategoryID == book.CategoryID && x.ID != book.ID).AsNoTracking().ToListAsync();

        var list2 = tempList.OrderBy(s => Random.Shared.Next()).Take(10).ToList();

        var list3 = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).AsSplitQuery().Where(x => x.Book.Author == book.Book.Author && x.ID != book.ID).AsNoTracking().Take(10).ToListAsync();

        return (OtherEditions: list1, BooksYouMightEnjoy: list2, OtherBooksBySameAuthor: list3);
    }

    public bool CheckIfHasStock(int bookID)
    {
        var value = _dataContext.BookEditions.Include(s => s.BooksInStock).AsSplitQuery().Where(x => x.ID == bookID).Any(q => q.BooksInStock.Any(a => a.AvailableStock >= 1));

        return value;
    }

    public async Task<BookEdition> GetByIdWithBookAsync(int id)
    {
        return await _dataContext.BookEditions.Include(s => s.Book).AsSplitQuery().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id);
    }

    public async Task<HomeCarouselViewModel> GetHomeCarouselItems(int takeNr)
    {

        DateTime currentDate = DateTime.Now;
        DateTime thirtyDaysAgo = currentDate.AddDays(-30);

        var tempList = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).AsSplitQuery().AsNoTracking().ToListAsync();

        return
        new HomeCarouselViewModel()
        {
            NewReleases = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).AsSplitQuery().Where(x => x.PublishDate >= thirtyDaysAgo).OrderByDescending(e => e.PublishDate).AsNoTracking().Take(takeNr).ToListAsync(),
            FeaturedBooks = tempList.OrderBy(s => Random.Shared.Next()).Take(takeNr).ToList(),
            Fantasy = await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.Ratings).Include(w => w.Category).AsSplitQuery().Where(x => x.Category.Name == "Fantasy").AsNoTracking().Take(takeNr).ToListAsync(),
        };
    }

    public async Task<BookEdition> GetByIdForCheckoutAsync(int id)
    {
        return await _dataContext.BookEditions.Include(s => s.Book).Include(q => q.BooksInStock).ThenInclude(x => x.Library).AsSplitQuery().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id);
    }

    public bool CheckIfBorrowed(int bookID, string UserID)
    {
        var value = _dataContext.BookEditions.Include(s => s.Loans).AsSplitQuery().Where(x => x.ID == bookID).Any(q => q.Loans.Any(a => a.UserID == UserID));

        return value;
    }

}
