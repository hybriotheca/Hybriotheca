using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IMailHelper
    {
        bool SendConfirmationEmail(AppUser user, string tokenUrl);
        bool SendEmail(string to, string subject, string body);
        bool SendPasswordResetEmail(AppUser user, string tokenUrl);
    }
}