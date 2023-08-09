using Hybriotheca.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IUserHelper
    {
        Task<IdentityResult> AddUserAsync(AppUser user, string password);
        Task AddUserToRoleAsync(AppUser user, string role);
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string oldPassword, string newPassword);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task ConfirmEmailAsync(AppUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<bool> IsUserInRoleAsync(AppUser user, string role);
        Task<SignInResult> LoginAsync(AppUser user, string password, bool rememberMe);
        Task LogoutAsync();
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string password);
        Task RollbackRegisteredUserAsync(AppUser user);
        Task<IdentityResult> UpdateUserAsync(AppUser user);
    }
}