using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
    }
}
