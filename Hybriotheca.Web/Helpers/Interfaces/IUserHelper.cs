﻿using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IUserHelper
    {
        Task<IdentityResult> AddUserAsync(AppUser user);
        Task<IdentityResult> AddUserAsync(AppUser user, string password);
        Task AddUserToRoleAsync(AppUser user, string role);
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string oldPassword, string newPassword);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task ConfirmEmailAsync(AppUser user);
        Task DeleteUserAsync(AppUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<IEnumerable<SelectListItem>> GetComboRolesAsync();
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<string> GetUserRoleAsync(AppUser user);
        Task<IEnumerable<AppUser>> GetUsersInRoleAsync(string roleName);
        Task<bool> IsUserInRoleAsync(AppUser user, string role);
        Task<SignInResult> LoginAsync(AppUser user, string password, bool rememberMe);
        Task LogoutAsync();
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string password);
        Task SetUserRoleAsync(AppUser user, string newRole);
        Task<IdentityResult> UpdateUserAsync(AppUser user);
        Task<bool> UserExistsAsync(string id);
    }
}