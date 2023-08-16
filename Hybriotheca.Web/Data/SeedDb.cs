using Hybriotheca.Web.Data.Entities;
﻿using Hybriotheca.Web.Helpers.Interfaces;
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

            await SeedLibrariesAsync();
            await SeedSubscriptions();

            await _context.SaveChangesAsync();
        }


        private async Task SeedLibrariesAsync()
        {
            string[] libraries = _configuration["SeedDb:Libraries:Names"].Split(',');

            foreach (string library in libraries)
            {
                if (!await _context.Libraries.AnyAsync(l => l.Name == library))
                {
                    await _context.Libraries.AddAsync(new Library
                    {
                        Name = library,
                        Location = _configuration[$"SeedDb:Libraries:{library}:Location"],
                        Contact = _configuration[$"SeedDb:Libraries:{library}:Contact"],
                    });
                }
            }
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

        private async Task SeedSubscriptions()
        {
            string[] subscriptionsNames = _configuration["SeedDb:Subscriptions:Names"].Split(',');

            foreach (string subscriptionName in subscriptionsNames)
            {
                // Check Subscription exists.
                if (!await _context.Subscriptions.AnyAsync(s => s.Name == subscriptionName))
                {
                    // If Subscription doesn't exist, create it.
                    await _context.Subscriptions.AddAsync(new Subscription
                    {
                        Name = subscriptionName,
                        Details = _configuration[$"SeedDb:Subscriptions:{subscriptionName}"],
                    });
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
                    user = new AppUser
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
