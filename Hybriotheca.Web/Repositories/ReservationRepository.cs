using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    private readonly DataContext _dataContext;

    public ReservationRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
