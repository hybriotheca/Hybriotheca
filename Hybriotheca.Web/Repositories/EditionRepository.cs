using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Entities;
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
