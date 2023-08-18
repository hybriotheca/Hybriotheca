using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private readonly DataContext _dataContext;

        public GenericRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task CreateAsync(T entity)
        {
            await _dataContext.Set<T>().AddAsync(entity);
            await SaveAllAsync();
        }

        private async Task<bool> SaveAllAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public async Task DeleteAsync(T entity)
        {
            _dataContext.Set<T>().Remove(entity);
            await SaveAllAsync();
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _dataContext.Set<T>().AsNoTracking().AnyAsync(x => x.ID == id);
        }

        public IQueryable<T> GetAll()
        {
            return _dataContext.Set<T>().AsNoTracking();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dataContext.Set<T>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dataContext.Set<T>().Update(entity);
            await SaveAllAsync();
        }
    }
}
