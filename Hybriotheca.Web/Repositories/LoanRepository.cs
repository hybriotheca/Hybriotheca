using Hybriotheca.Web.Data;
using Hybriotheca.Web.Repositories.Entities;
using Hybriotheca.Web.Repositories.Interfaces;

namespace Hybriotheca.Web.Repositories;

public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    private readonly DataContext _dataContext;

    public LoanRepository(DataContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
