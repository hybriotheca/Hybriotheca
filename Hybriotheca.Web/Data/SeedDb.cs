using Hybriotheca.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        private readonly IConfiguration _configuration;
        private readonly IUserHelper _userHelper;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(
            DataContext context,
            IConfiguration configuration,
            IUserHelper userHelper,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _configuration = configuration;
            _userHelper = userHelper;
            _roleManager = roleManager;
        }


        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await SeedRoles();
            await SeedUsers();

            await _context.SaveChangesAsync();
        }


        private async Task SeedRoles()
        {
            string[] roles = _configuration["SeedDb:Roles"].Split(',');

            foreach (string role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsers()
        {
            string[] defaultUserNames = _configuration["SeedDb:Users:DefaultUsers"].Split(',');

            foreach (string userName in defaultUserNames)
            {
                await SeedOneUserAsync(userName);
            }

            async Task SeedOneUserAsync(string name)
            {
                var email = _configuration[$"SeedDb:Users:{name}:Email"];
                
                var user = await _userHelper.GetUserByEmailAsync(email);
                if (user == null)
                {
                    user = new Entities.AppUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                    };

                    var password = _configuration[$"SeedDb:Users:{name}:Password"];

                    await _userHelper.AddUserAsync(user, password);
                }

                if (!await _userHelper.IsUserInRoleAsync(user, "Admin"))
                {
                    await _userHelper.AddUserToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
