using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        public SeedDb(DataContext context)
        {
            _context = context;
        }


        public void Seed()
        {
            _context.Database.Migrate();

            // Seed here.

            _context.SaveChanges();
        }
    }
}
