using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories
{
    public class EditionRepository : GenericRepository<Edition>, IEditionRepository
    {
        private readonly DataContext _dataContext;

        public EditionRepository(DataContext dataContext) : base(dataContext)
        {
            _dataContext = dataContext;
        }


    }
}
