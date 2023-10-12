using Hybriotheca.Web.Data.Entities;
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
        Task ConfirmEmailAsync(AppUser user);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task<IdentityResult> DeleteUserAsync(AppUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        IQueryable<AppUser> GetAllUsers();
        Task<IEnumerable<SelectListItem>> GetComboCustomersAsync();
        Task<IEnumerable<SelectListItem>> GetComboRolesAsync();
        Task<int?> GetMainLibraryIdOfUserAsync(string userEmail);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<string?> GetUserRoleAsync(string userEmail);
        Task<bool> IsConstrainedAsync(string id);
        Task<bool> IsThereAnyOtherAdminAsync(string id);
        Task<bool> IsUserInRoleAsync(AppUser user, string role);
        Task<SignInResult> LoginAsync(AppUser user, string password, bool rememberMe);
        Task LogoutAsync();
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string password);
        Task SetUserRoleAsync(AppUser user, string newRole);
        Task<IdentityResult> UpdateUserAsync(AppUser user);
        Task<bool> UserExistsAsync(string id);
    }
}