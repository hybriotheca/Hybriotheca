using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Models.Search;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

    public async Task UpdateKeepCoverImageAsync(BookEdition bookEdition)
    {
        _dataContext.Set<BookEdition>().Update(bookEdition);
        _dataContext.Entry(bookEdition).Property(b => b.CoverImageID).IsModified = false;
        await _dataContext.SaveChangesAsync();
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
        var editionsQuery = _dataContext.BookEditions.AsQueryable();

        if (viewModel.Categories is not null && viewModel.Categories.Any())
        {
            editionsQuery = editionsQuery.Include(s => s.Category).AsSplitQuery().Where(x => viewModel.Categories.Contains(x.Category.Name));

            for (int i = 0; i < viewModel.Categories.Count; i++)
            {
                if (i == 0)
                {
                    searchUrl = string.Concat(searchUrl, $"categories={viewModel.Categories[0]}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&categories={viewModel.Categories[i]}");
                }
            }

            _hasExistingParams = true;
        }

        if (viewModel.Rating is not null && viewModel.Rating.Any())
        {
            editionsQuery = editionsQuery.Include(s => s.Ratings).AsSplitQuery().Where(x => viewModel.Rating.Contains(((int)x.Ratings.Average(w => w.BookRating))));

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
                    searchUrl = string.Concat(searchUrl, $"formats={viewModel.Formats[0]}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&formats={viewModel.Formats[i]}");
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
                    searchUrl = string.Concat(searchUrl, $"lang={viewModel.Languages[0]}");
                }
                else
                {
                    searchUrl = string.Concat(searchUrl, $"&lang={viewModel.Languages[i]}");
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
            editionsQuery = editionsQuery.Where(
            s => s.EditionTitle.Contains(viewModel.SearchTerm) ||
            s.Book.Author.Contains(viewModel.SearchTerm) ||
            s.ISBN.Contains(viewModel.SearchTerm) ||
            s.Publisher.Contains(viewModel.SearchTerm));

            if (!_hasExistingParams)
            {
                searchUrl = string.Concat(searchUrl, $"searchTerm={viewModel.SearchTerm}");
            }
            else
            {
                searchUrl = string.Concat(searchUrl, $"&searchTerm={viewModel.SearchTerm}");
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
                editionsQuery = editionsQuery.Include(a => a.Ratings).AsSplitQuery().OrderByDescending(s => ((int)s.Ratings.Average(w => w.BookRating)));  
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
}
