using Hybriotheca.Web.Models.Entities;

namespace Hybriotheca.Web.Models.Account
{
    public class UserSettingsViewModel
    {
        public UpdateUserViewModel UserViewModel { get; set; }
        public ChangePasswordViewModel PasswordViewModel { get; set; }
    }
}
