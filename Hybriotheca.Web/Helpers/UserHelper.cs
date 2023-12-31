﻿using System.Data;
using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public UserHelper(
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }


        public async Task<IdentityResult> AddUserAsync(AppUser user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> AddUserAsync(AppUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(AppUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> ChangePasswordAsync(
            AppUser user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task ConfirmEmailAsync(AppUser user)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> DeleteUserAsync(AppUser user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(AppUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public IQueryable<AppUser> GetAllUsers()
        {
            return _userManager.Users.AsNoTracking();
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCustomersAsync()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");

            return customers.Select(customer => new SelectListItem
            {
                Text = customer.UserName,
                Value = customer.Id,
            });
        }

        public async Task<IEnumerable<SelectListItem>> GetComboRolesAsync()
        {
            return await _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
            }).ToListAsync();
        }

        public async Task<int?> GetMainLibraryIdOfUserAsync(string userEmail)
        {
            return await _userManager.Users
                .Where(user => user.Email == userEmail)
                .Select(user => user.MainLibraryID)
                .SingleOrDefaultAsync();
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<AppUser?> GetUserByIdForProfileAsync(string id)
        {
            return await _userManager.Users
                .Include(user => user.Subscription)
                .Include(user => user.Ratings)
                .ThenInclude(rating => rating.BookEdition)
                .ThenInclude(bookEdition => bookEdition.Book)
                .AsSplitQuery()
                .SingleOrDefaultAsync(user => user.Id == id);
        }

        public async Task<AppUser?> GetUserByIdForLoanAsync(string id)
        {
            return await _userManager.Users
                .Include(user => user.Loans)
                .ThenInclude(loan => loan.BookEdition)
                .ThenInclude(bookEdition => bookEdition.Book)
                .Include(user => user.Loans)
                .ThenInclude(loan => loan.Library)
                .AsSplitQuery()
                .SingleOrDefaultAsync(user => user.Id == id);
        }

        public async Task<string?> GetUserRoleAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return null;

            return (await _userManager.GetRolesAsync(user))[0];
        }

        public async Task<bool> IsConstrainedAsync(string id)
        {
            return await _userManager.Users
                .Where(user => user.Id == id)
                .AnyAsync(user => user.Loans.Any() || user.Ratings.Any() || user.Reservations.Any());
        }

        public async Task<bool> IsThereAnyOtherAdminAsync(string id)
        {
            return await _userManager.Users.AnyAsync(user => user.Role == "Admin" && user.Id != id);
        }

        public async Task<bool> IsUserInRoleAsync(AppUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<SignInResult> LoginAsync(AppUser user, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task SetUserRoleAsync(AppUser user, string newRole)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            await _userManager.AddToRoleAsync(user, newRole);
        }

        public async Task<IdentityResult> UpdateUserAsync(AppUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> UserExistsAsync(string id)
        {
            return await _userManager.Users.AsNoTracking().AnyAsync(user => user.Id == id);
        }

        public async Task<UpdateUserViewModel?> SelectUserViewModel(string email)
        {
            return await _userManager.Users
                .Where(user => user.Email == email)
                .Select(user => new UpdateUserViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    HasPhoto = user.PhotoId != Guid.Empty,
                    PhotoFullPath = user.PhotoFullPath,
                    MainLibraryID = user.MainLibraryID ?? 0,
                    SubscriptionName = user.Subscription.Name
                })
                .SingleOrDefaultAsync();
        }
    }
}
