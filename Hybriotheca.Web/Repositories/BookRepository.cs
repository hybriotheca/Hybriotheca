﻿using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    private readonly DataContext _dataContext;

    public BookRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<IEnumerable<SelectListItem>> GetComboBooksAsync()
    {
        return await _dataContext.Books.Select(c => new SelectListItem
        {
            Text = c.OriginalTitle,
            Value = c.ID.ToString()
        }).ToListAsync();
    }

    public async Task<bool> IsConstrainedAsync(int id)
    {
        return await _dataContext.Books
            .Where(book => book.ID == id)
            .AnyAsync(book => book.Editions.Any());
    }
}
